using Back_EndAPI.Data;
using ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Services
{
    public interface IInventoryReportService
    {
        Task<InventoryResponseDto> GetInventoryAsync(int? productId = null);
    }

    public class InventoryReportService : IInventoryReportService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<InventoryReportService> _logger;

        public InventoryReportService(AppDbContext dbContext, ILogger<InventoryReportService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<InventoryResponseDto> GetInventoryAsync(int? productId = null)
        {
            try
            {
                var response = new InventoryResponseDto
                {
                    LastUpdated = DateTime.UtcNow
                };
                // Get all received items
                var receivedItems = await _dbContext.ReceivedItems
                    .Include(ri => ri.SkuNumberNavigation)
                    .ToListAsync();

                if (productId.HasValue)
                {
                    receivedItems = receivedItems
                        .Where(ri => ri.SkuNumber == productId.Value)
                        .ToList();
                }

                if (!receivedItems.Any())
                {
                    _logger.LogInformation($"No inventory found for productId: {productId}");
                    return response;
                }

                // Group by SKU and calculate inventory
                var groupedBySkU = receivedItems.GroupBy(ri => ri.SkuNumber);
                foreach (var skuGroup in groupedBySkU)
                {
                    var item = skuGroup.First();
                    var product = item.SkuNumberNavigation;

                    int totalDeposited = 0;
                    int totalWithdrawn = 0;
                    var storageLocations = new List<StorageLocationDto>();

                    // Calculate deposits and withdrawals
                    foreach (var receivedItem in skuGroup)
                    {
                        var deposits = await _dbContext.TransferRecords
                            .Where(tr => tr.Receiveditemid == receivedItem.Id && tr.Deposit == true)
                            .SumAsync(tr => tr.Qty ?? 0);

                        var withdrawals = await _dbContext.TransferRecords
                            .Where(tr => tr.Receiveditemid == receivedItem.Id && tr.Withdrawal == true)
                            .SumAsync(tr => tr.Qty ?? 0);

                        totalDeposited += deposits;
                        totalWithdrawn += withdrawals;
                        // Get storage locations with quantities
                        var storageWithQty = await _dbContext.TransferRecords
                            .Where(tr => tr.Receiveditemid == receivedItem.Id && tr.Deposit == true)
                            .Include(tr => tr.Storagelocation)
                            .GroupBy(tr => tr.Storagelocationid)
                            .Select(g => new
                            {
                                BinId = g.Key,
                                Quantity = g.Sum(tr => tr.Qty ?? 0),
                                BinLocation = g.First().Storagelocation != null ?
                                    $"Bin {g.First().Storagelocation.Id}" : "Unknown"
                            })
                            .ToListAsync();
                        foreach (var storage in storageWithQty)
                        {
                            if (storage.BinId.HasValue)
                            {
                                var withdrawnFromBin = await _dbContext.TransferRecords
                                    .Where(tr => tr.Receiveditemid == receivedItem.Id &&
                                               tr.Withdrawal == true &&
                                               tr.Storagelocationid == storage.BinId)
                                    .SumAsync(tr => tr.Qty ?? 0);

                                var netQuantity = storage.Quantity - withdrawnFromBin;
                                if (netQuantity > 0)
                                {
                                    storageLocations.Add(new StorageLocationDto
                                    {
                                        BinId = storage.BinId.Value,
                                        BinLocation = storage.BinLocation,
                                        Quantity = netQuantity
                                    });
                                }
                            }
                        }
                    }
                    int quantityAvailable = totalDeposited - totalWithdrawn;

                    var inventoryItem = new InventoryItemDto
                    {
                        SkuNumber = (int)skuGroup.Key,
                        ProductName = product?.Name ?? "Unknown",
                        Description = product?.Description,
                        TotalDeposited = totalDeposited,
                        TotalWithdrawn = totalWithdrawn,
                        QuantityAvailable = Math.Max(0, quantityAvailable),
                        StorageLocations = storageLocations
                    };

                    response.Items.Add(inventoryItem);
                    response.TotalQuantityAvailable += inventoryItem.QuantityAvailable;
                }

                response.TotalItems = response.Items.Count;

                _logger.LogInformation($"Inventory report generated: {response.Items.Count} products, {response.TotalQuantityAvailable} total quantity");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating inventory report: {ex.Message}");
                throw;
            }
        }
    }

}