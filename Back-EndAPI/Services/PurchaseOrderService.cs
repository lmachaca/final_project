using Back_EndAPI.Data;
using ClassLibrary.DTOs;
using Back_EndAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Services;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderResponseDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequestDto request);
}

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(AppDbContext dbContext, ILogger<PurchaseOrderService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PurchaseOrderResponseDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequestDto request)
    {
        // 1. Validate items exist
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("Purchase order must contain at least one item");
        }

        // 2. Validate all items have valid quantities
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"Quantity for Product ID {item.ProductId} must be greater than 0");
            }

            // 3. Validate product exists
            var productExists = await _dbContext.Items.AnyAsync(i => i.SkuNumber == item.ProductId);
            if (!productExists)
            {
                throw new ArgumentException($"Product with ID {item.ProductId} does not exist");
            }
        }

        // 4. Validate vendor exists
        var vendorExists = await _dbContext.Vendors.AnyAsync(v => v.Id == request.VendorId);
        if (!vendorExists)
        {
            throw new ArgumentException($"Vendor with ID {request.VendorId} not found");
        }

        // 5. Create purchase order
        var purchaseOrder = new PurchaseOrder
        {
            DateOrdered = request.DateOrdered,
            Vendorid = request.VendorId
        };

        _dbContext.PurchaseOrders.Add(purchaseOrder);
        await _dbContext.SaveChangesAsync();

        // 6. Create ordered items
        foreach (var itemDto in request.Items)
        {
            var orderedItem = new OrderedItem
            {
                PurchaseId = purchaseOrder.Id,
                SkuNumber = itemDto.ProductId,
                Qty = itemDto.Quantity,
                PriceToBePaid = itemDto.UnitPrice ?? 0
            };
            _dbContext.OrderedItems.Add(orderedItem);
        }
        //transaction
        //using var transaction = await _dbContext.Database.BeginTransactionAsync();
        //try
        //{
        //save/modifying thing 1
        //save/modifyibg thing 2 
        //  await transaction.CommitAsync();
        //}
        //catch { 
        //   await transaction.RollbackAsync();
        // throw;
        //}

        //EF
        var purchase = new PurchaseOrder(); // create the purchse
        var log = new Discrepancy();
       // var otherThing = new CharTheirCard();
        //_dbContext.PurchaseOrders.Add(purchase); // add it in the context
        //_dbContext.Discrepancies.Add(log);
        //_dbContext.Payment.Add(otherThing);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Purchase order {purchaseOrder.Id} created with {request.Items.Count} items");

        // 7. Return response with CREATED status
        return new PurchaseOrderResponseDto
        {
            Id = purchaseOrder.Id,
            DateOrdered = purchaseOrder.DateOrdered,
            VendorId = purchaseOrder.Vendorid,
            Status = "CREATED"
        };
    }
   
}