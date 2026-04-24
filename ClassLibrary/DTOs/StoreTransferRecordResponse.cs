using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class StoreTransferRecordResponseDto
    {
        public int TransferRecordId { get; set; }
        public int ReceivedItemId { get; set; }
        public int StorageLocationId { get; set; }
        public int Quantity { get; set; }
        public bool Deposit { get; set; } = true;
        public DateTime StoredDateTime { get; set; }
        public string Message { get; set; } = "Item stored successfully";
    }
}
