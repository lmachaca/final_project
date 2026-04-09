using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class CreatePurchaseOrderRequestDto
    {
        [Required(ErrorMessage = "Date ordered is required")]
        public DateOnly DateOrdered { get; set; }

        [Required(ErrorMessage = "Vendor ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Vendor ID must be a positive number")]
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Items must be provided")]
        [MinLength(1, ErrorMessage = "Purchase order must contain at least one item")]
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
    }
    public class PurchaseOrderItemDto
    {
        [Required(ErrorMessage = "Product ID (SKU) is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be a positive number")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
        public decimal? UnitPrice { get; set; }
    }
}
