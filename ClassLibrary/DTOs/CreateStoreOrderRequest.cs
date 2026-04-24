using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class CreateStoreOrderRequestDto
    {
        [Required(ErrorMessage = "Supplier ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Supplier ID must be a positive number")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Items must be provided")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<StoreOrderItemDto> Items { get; set; } = new();

        public DateOnly? OrderDate { get; set; }
    }

    public class StoreOrderItemDto
    {
        [Required(ErrorMessage = "Item ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Item ID must be a positive number")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")]
        public decimal? ActualPrice { get; set; }
    }
}
