using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class ReceiveShipmentResponseDto
    {
        public int ShipmentId { get; set; }
        public DateOnly ReceivedDate { get; set; }
        public int TotalItemsReceived { get; set; }
        public List<ReceivedItemDetailDto> Items { get; set; } = new();
        public List<DiscrepancyDto>? Discrepancies { get; set; }
    }
    public class ReceivedItemDetailDto
    {
        public int SkuNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityReceived { get; set; }
        public int? QuantityExpected { get; set; }
        public int? Difference { get; set; }
    }
    public class DiscrepancyDto
    {
        public int SkuNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Expected { get; set; }
        public int Received { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
