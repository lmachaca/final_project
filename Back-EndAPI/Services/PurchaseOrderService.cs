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

    public PurchaseOrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseOrderResponseDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequestDto request)
    {
        // Validate vendor exists
        var vendorExists = await _dbContext.Vendors.AnyAsync(v => v.Id == request.VendorId);
        if (!vendorExists)
        {
            throw new ArgumentException($"Vendor with ID {request.VendorId} not found");
        }

        var purchaseOrder = new PurchaseOrder
        {
            DateOrdered = request.DateOrdered,
            Vendorid = request.VendorId
        };

        _dbContext.PurchaseOrders.Add(purchaseOrder);
        await _dbContext.SaveChangesAsync();

        // Retrieve the created order with vendor details
        var createdOrder = await _dbContext.PurchaseOrders
            .Include(po => po.Vendor)
            .FirstAsync(po => po.Id == purchaseOrder.Id);

        return MapToResponseDto(createdOrder);
    }

    private static PurchaseOrderResponseDto MapToResponseDto(PurchaseOrder purchaseOrder)
    {
        return new PurchaseOrderResponseDto
        {
            Id = purchaseOrder.Id,
            DateOrdered = purchaseOrder.DateOrdered,
            VendorId = purchaseOrder.Vendorid
            
        };
    }
}