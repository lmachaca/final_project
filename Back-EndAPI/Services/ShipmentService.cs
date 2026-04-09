using Back_EndAPI.Data;
using Back_EndAPI.Entities;
using ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Services
{
    public interface IShipmentService
    {
        Task<ReceiveShipmentResponseDto> ReceiveShipmentAsync(ReceiveShipmentRequestDto request);
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
            // 1. Validate shipment exists
            var shipment = await _dbContext.ReceivedShipments
                .Include(s => s.PurchaseOrder)
                .ThenInclude(po => po!.OrderedItems)
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId);

            if (shipment == null)
            {
                throw new ArgumentException($"Shipment with ID {request.ShipmentId} not found");
            }

            // 2. Validate all items and quantities
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
            // 3. Create received items and update inventory
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

            // 4. Update shipment date if provided
            if (request.ReceivedDate.HasValue)
            {
                shipment.Date = request.ReceivedDate;
            }
            else
            {
                shipment.Date = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            // 5. Save all changes
            await _dbContext.SaveChangesAsync();

            // 6. Build response
            return await BuildResponseAsync(shipment, receivedItems, discrepancies);
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
    }

}
