using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class ReceiveShipmentRequestDto
    {
        [Required(ErrorMessage = "Shipment ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Shipment ID must be a positive number")]
        public int ShipmentId { get; set; }

        [Required(ErrorMessage = "Items must be provided")]
        [MinLength(1, ErrorMessage = "At least one item must be received")]
        public List<ReceivedItemDto> Items { get; set; } = new();

        public DateOnly? ReceivedDate { get; set; }
    }
    public class ReceivedItemDto
    {
        [Required(ErrorMessage = "SKU number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "SKU number must be a positive number")]
        public int SkuNumber { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price paid cannot be negative")]
        public decimal? ActualPricePaid { get; set; }
    }
}
