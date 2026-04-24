using Back_EndAPI.Data;
using Back_EndAPI.Entities;
using ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace Back_EndAPI.Services
{
    public interface IStoreOrderService
    {
        Task<StoreOrderResponseDto> CreateOrderAsync(CreateStoreOrderRequestDto request);
        Task<StoreOrderResponseDto> PickOrderAsync(int orderId);
        Task<StoreOrderResponseDto> PackOrderAsync(int orderId);
        Task<StoreOrderResponseDto> ShipOrderAsync(int orderId);
    }

    public class StoreOrderService : IStoreOrderService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<StoreOrderService> _logger;

        public StoreOrderService(AppDbContext dbContext, ILogger<StoreOrderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<StoreOrderResponseDto> CreateOrderAsync(CreateStoreOrderRequestDto request)
        {
            // 1. Validate items exist
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new ArgumentException("Order must contain at least one item");
            }
            // 2. Validate all items
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Quantity for Item ID {item.ItemId} must be greater than 0");
                }

                var itemExists = await _dbContext.Items.AnyAsync(i => i.SkuNumber == item.ItemId);
                if (!itemExists)
                {
                    throw new ArgumentException($"Item with ID {item.ItemId} does not exist");
                }
            }
            // 3. Validate customer exists
            var customerExists = await _dbContext.Customers.AnyAsync(c => c.Id == request.SupplierId);
            if (!customerExists)
            {
                throw new ArgumentException($"Customer with ID {request.SupplierId} not found");
            }

            // 4. Create order
            var order = new CustomerOrder
            {
                CustomerId = request.SupplierId,
                DateTimeOrdered = request.OrderDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
            };
            _dbContext.CustomerOrders.Add(order);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Customer Order {order.Id} created with status CREATED");

            return new StoreOrderResponseDto
            {
                Id = order.Id,
                SupplierId = order.CustomerId,
                DatePurchased = order.DateTimeOrdered,
                Status = "CREATED",
                Items = new List<StoreOrderItemResponseDto>()
            };
        }
        public async Task<StoreOrderResponseDto> PickOrderAsync(int orderId)
        {
            var order = await _dbContext.CustomerOrders.FindAsync(orderId);
            if (order == null)
                throw new ArgumentException($"Order with ID {orderId} not found");

            // Get sold items for this order
            var soldItems = await _dbContext.SoldItems
                .Where(si => si.CustomerOrderId == orderId)
                .ToListAsync();

            if (!soldItems.Any())
                throw new ArgumentException("Order contains no items to pick");

            // IDEMPOTENCY CHECK: Get all received item IDs from sold items
            var soldItemIds = soldItems.Select(si => si.Id).ToList();

            var alreadyPickedWithdrawals = await _dbContext.TransferRecords
                .Where(tr => tr.Withdrawal == true &&
                       (tr.Receiveditemid.HasValue && soldItemIds.Contains(tr.Receiveditemid.Value)))
                .AnyAsync();

            if (alreadyPickedWithdrawals)
            {
                _logger.LogWarning($"Order {orderId} was already picked - preventing duplicate withdrawal");
                throw new ArgumentException("Order has already been picked. Cannot pick twice (idempotency protection)");
            }

            // Check inventory and bin constraint for each item
            foreach (var soldItem in soldItems)
            {
                // Get received items (inventory) for this SKU
                var receivedItems = await _dbContext.ReceivedItems
                    .Where(ri => ri.SkuNumber == soldItem.SkuNumber)
                    .ToListAsync();

                if (!receivedItems.Any())
                {
                    throw new ArgumentException($"No inventory for SKU {soldItem.SkuNumber}");
                }

                // Calculate available inventory (deposits - withdrawals)
                int availableQty = 0;
                foreach (var receivedItem in receivedItems)
                {
                    var deposits = await _dbContext.TransferRecords
                        .Where(tr => tr.Receiveditemid == receivedItem.Id && tr.Deposit == true)
                        .SumAsync(tr => tr.Qty ?? 0);

                    var withdrawals = await _dbContext.TransferRecords
                        .Where(tr => tr.Receiveditemid == receivedItem.Id && tr.Withdrawal == true)
                        .SumAsync(tr => tr.Qty ?? 0);

                    availableQty += (deposits - withdrawals);
                }

                // Negative inventory protection
                if (availableQty < (soldItem.Qty))
                {
                    throw new ArgumentException($"Insufficient inventory for SKU {soldItem.SkuNumber}. Required: {soldItem.Qty}, Available: {availableQty}");
                }

            }

            // Create withdrawal records for picked items (IDEMPOTENT)
            foreach (var soldItem in soldItems)
            {
                var receivedItem = await _dbContext.ReceivedItems
                    .FirstOrDefaultAsync(ri => ri.SkuNumber == soldItem.SkuNumber);

                if (receivedItem != null)
                {
                    var withdrawalRecord = new TransferRecord
                    {
                        Receiveditemid = receivedItem.Id,
                       // Storagelocationid = soldItem.BinId,
                        Withdrawal = true,
                        Deposit = false,
                        Qty = soldItem.Qty,
                        Datetime = DateTime.UtcNow
                    };

                    _dbContext.TransferRecords.Add(withdrawalRecord);
                }
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Order {orderId} picked, withdrawal records created");

            return new StoreOrderResponseDto
            {
                Id = order.Id,
                SupplierId = order.CustomerId,
                DatePurchased = order.DateTimeOrdered,
                Status = "PICKED",
                Items = new List<StoreOrderItemResponseDto>()
            };
        }

        public async Task<StoreOrderResponseDto> PackOrderAsync(int orderId)
        {
            var order = await _dbContext.CustomerOrders.FindAsync(orderId);

            if (order == null)
            {
                throw new ArgumentException($"Order with ID {orderId} not found");
            }

            _logger.LogInformation($"Order {orderId} status updated to PACKED");

            return new StoreOrderResponseDto
            {
                Id = order.Id,
                SupplierId = order.CustomerId,
                DatePurchased = order.DateTimeOrdered,
                Status = "PACKED",
                Items = new List<StoreOrderItemResponseDto>()
            };
        }
        public async Task<StoreOrderResponseDto> ShipOrderAsync(int orderId)
        {
            var order = await _dbContext.CustomerOrders.FindAsync(orderId);

            if (order == null)
            {
                throw new ArgumentException($"Order with ID {orderId} not found");
            }

            _logger.LogInformation($"Order {orderId} status updated to SHIPPED");

            return new StoreOrderResponseDto
            {
                Id = order.Id,
                SupplierId = order.CustomerId,
                DatePurchased = order.DateTimeOrdered,
                Status = "SHIPPED",
                Items = new List<StoreOrderItemResponseDto>()
            };
        }
    }

}
