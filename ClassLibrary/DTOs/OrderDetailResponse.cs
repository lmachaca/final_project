using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class OrderDetailResponseDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateOnly? DateOrdered { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public List<OrderItemDetailDto> Items { get; set; } = new();
        public ShippingInfoDto? ShippingInfo { get; set; }
        public decimal? TotalValue { get; set; }
    }
    public class OrderItemDetailDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string Status { get; set; } = "CREATED";
    }

    public class ShippingInfoDto
    {
        public int? CarrierId { get; set; }
        public string? CarrierName { get; set; }
        public decimal? ShippingFee { get; set; }
        public DateTime? ShippedDate { get; set; }
        public string TrackingStatus { get; set; } = string.Empty;
    }
}
