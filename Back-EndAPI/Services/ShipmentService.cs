using Back_EndAPI.Data;
using Back_EndAPI.Entities;
using Back_EndAPI.Services.Exceptions;
using ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Services;

public interface IShipmentService
{
    Task<ReceiveShipmentResponseDto> ReceiveShipmentAsync(ReceiveShipmentRequestDto request);
    Task<CreateShipmentResponseDto> CreateShipmentAsync(CreateShipmentRequestDto request);
}

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ShipmentService> _logger;

    public ShipmentService(AppDbContext dbContext, ILogger<ShipmentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ReceiveShipmentResponseDto> ReceiveShipmentAsync(ReceiveShipmentRequestDto request)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Validate shipment exists
            var shipment = await _dbContext.ReceivedShipments
                .Include(s => s.PurchaseOrder)
                .ThenInclude(po => po!.OrderedItems)
                .Include(s => s.ReceivedItems)
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId);

            if (shipment == null)
            {
                throw new ShipmentNotFoundException(request.ShipmentId);
            }

            // 2. Check if shipment is already received (has ReceivedItems)
            if (shipment.ReceivedItems.Any())
            {
                throw new ShipmentAlreadyReceivedException(request.ShipmentId);
            }

            // 3. Validate all items and quantities
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Quantity for SKU {item.SkuNumber} must be greater than 0");
                }

                // Check if item exists
                var itemExists = await _dbContext.Items.AnyAsync(i => i.SkuNumber == item.SkuNumber);
                if (!itemExists)
                {
                    throw new ArgumentException($"Item with SKU {item.SkuNumber} does not exist");
                }
            }

            // 4. Create received items and update inventory
            var receivedItems = new List<ReceivedItem>();
            var discrepancies = new List<DiscrepancyDto>();

            foreach (var itemDto in request.Items)
            {
                // Check expected quantity from ordered items
                var orderedItem = shipment.PurchaseOrder?.OrderedItems
                    .FirstOrDefault(oi => oi.SkuNumber == itemDto.SkuNumber);

                // Create received item
                var receivedItem = new ReceivedItem
                {
                    SkuNumber = itemDto.SkuNumber,
                    ShipmentId = shipment.Id,
                    Qty = itemDto.Quantity,
                    ActualPricePaid = itemDto.ActualPricePaid
                };

                _dbContext.ReceivedItems.Add(receivedItem);
                receivedItems.Add(receivedItem);

                // Compare with expected quantity
                if (orderedItem != null && orderedItem.Qty != itemDto.Quantity)
                {
                    var item = await _dbContext.Items.FindAsync(itemDto.SkuNumber);
                    discrepancies.Add(new DiscrepancyDto
                    {
                        SkuNumber = itemDto.SkuNumber,
                        ItemName = item?.Name ?? "Unknown",
                        Expected = orderedItem.Qty,
                        Received = itemDto.Quantity,
                        Message = itemDto.Quantity < orderedItem.Qty
                            ? $"Under received: expected {orderedItem.Qty}, received {itemDto.Quantity}"
                            : $"Over received: expected {orderedItem.Qty}, received {itemDto.Quantity}"
                    });
                }

                _logger.LogInformation($"Received {itemDto.Quantity} units of SKU {itemDto.SkuNumber}");
            }

            // 5. Update shipment date if provided
            if (request.ReceivedDate.HasValue)
            {
                shipment.Date = request.ReceivedDate;
            }
            else
            {
                shipment.Date = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            // 6. Save all changes
            await _dbContext.SaveChangesAsync();

            // COMMIT TRANSACTION
            await transaction.CommitAsync();

            _logger.LogInformation($"Shipment {request.ShipmentId} received successfully");

            // 7. Build response
            return await BuildResponseAsync(shipment, receivedItems, discrepancies);
        }
        catch (Exception ex)
        {
            // ROLLBACK ON ERROR
            await transaction.RollbackAsync();
            _logger.LogError($"Shipment receive transaction failed, rolling back: {ex.Message}");
            throw;
        }
    }

    private async Task<ReceiveShipmentResponseDto> BuildResponseAsync(
        ReceivedShipment shipment,
        List<ReceivedItem> receivedItems,
        List<DiscrepancyDto> discrepancies)
    {
        var response = new ReceiveShipmentResponseDto
        {
            ShipmentId = shipment.Id,
            ReceivedDate = shipment.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = "RECEIVED",
            TotalItemsReceived = receivedItems.Sum(ri => ri.Qty ?? 0),
            Items = new List<ReceivedItemDetailDto>()
        };

        foreach (var receivedItem in receivedItems)
        {
            var item = await _dbContext.Items.FindAsync(receivedItem.SkuNumber);
            var orderedItem = shipment.PurchaseOrder?.OrderedItems
                .FirstOrDefault(oi => oi.SkuNumber == receivedItem.SkuNumber);

            response.Items.Add(new ReceivedItemDetailDto
            {
                SkuNumber = (int)receivedItem.SkuNumber,
                ItemName = item?.Name ?? "Unknown",
                QuantityReceived = receivedItem.Qty ?? 0,
                QuantityExpected = orderedItem?.Qty,
                Difference = orderedItem != null ? (receivedItem.Qty - orderedItem.Qty) : null
            });
        }

        if (discrepancies.Any())
        {
            response.Discrepancies = discrepancies;
        }

        return response;
    }

    public async Task<CreateShipmentResponseDto> CreateShipmentAsync(CreateShipmentRequestDto request)
    {
        // 1. Validate purchase order exists
        var purchaseOrderExists = await _dbContext.PurchaseOrders
            .AnyAsync(po => po.Id == request.PurchaseOrderId);

        if (!purchaseOrderExists)
        {
            throw new ArgumentException($"Purchase Order with ID {request.PurchaseOrderId} not found");
        }

        // 2. Create received shipment
        var shipment = new ReceivedShipment
        {
            PurchaseOrderId = request.PurchaseOrderId,
            Date = request.ShipmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _dbContext.ReceivedShipments.Add(shipment);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Shipment {shipment.Id} created for Purchase Order {request.PurchaseOrderId}");

        return new CreateShipmentResponseDto
        {
            Id = shipment.Id,
            PurchaseOrderId = shipment.PurchaseOrderId ?? 0,
            ShipmentDate = shipment.Date
        };
    }
}
