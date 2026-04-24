using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class StoreOrderResponseDto
    {
        public int Id { get; set; }
        public int? SupplierId { get; set; }
        public DateOnly? DatePurchased { get; set; }
        public string Status { get; set; } = "CREATED";
        public List<StoreOrderItemResponseDto> Items { get; set; } = new();
    }

    public class StoreOrderItemResponseDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? ActualPrice { get; set; }
    }
}
