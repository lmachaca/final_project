using Back_EndAPI.Data;
using Back_EndAPI.Entities;
using ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Services
{
    public interface ITransferRecordService
    {
        Task<StoreTransferRecordResponseDto> StoreTransferRecordAsync(StoreTransferRecordRequestDto request);
    }

    public class TransferRecordService : ITransferRecordService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TransferRecordService> _logger;

        public TransferRecordService(AppDbContext dbContext, ILogger<TransferRecordService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<StoreTransferRecordResponseDto> StoreTransferRecordAsync(StoreTransferRecordRequestDto request)
        {
            // 1. Validate received item exists
            var receivedItem = await _dbContext.ReceivedItems.FindAsync(request.ReceivedItemId);
            if (receivedItem == null)
            {
                throw new ArgumentException($"Received Item with ID {request.ReceivedItemId} not found");
            }

            // 2. Validate storage location (bin) exists
            var storageLocation = await _dbContext.Bins.FindAsync(request.StorageLocationId);
            if (storageLocation == null)
            {
                throw new ArgumentException($"Storage Location (Bin) with ID {request.StorageLocationId} not found");
            }

            // 3. Validate quantity
            if (request.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            // 4. Create transfer record with deposit flag
            var transferRecord = new TransferRecord
            {
                Receiveditemid = request.ReceivedItemId,
                Storagelocationid = request.StorageLocationId,
                Qty = request.Quantity,
                Deposit = true,
                Withdrawal = false,
                Datetime = DateTime.UtcNow
            };

            _dbContext.TransferRecords.Add(transferRecord);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Transfer Record {transferRecord.Id} created: Item {request.ReceivedItemId} stored in bin {request.StorageLocationId} with quantity {request.Quantity}");

            return new StoreTransferRecordResponseDto
            {
                TransferRecordId = transferRecord.Id,
                ReceivedItemId = request.ReceivedItemId,
                StorageLocationId = request.StorageLocationId,
                Quantity = request.Quantity,
                Deposit = true,
                StoredDateTime = transferRecord.Datetime ?? DateTime.UtcNow,
                Message = "Item stored successfully"
            };
        }
    }

}
