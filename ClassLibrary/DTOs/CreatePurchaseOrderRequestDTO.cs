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
    }
}
