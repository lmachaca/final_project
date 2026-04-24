using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class CreateShipmentRequestDto
    {
        [Required(ErrorMessage = "Purchase Order ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Purchase Order ID must be a positive number")]
        public int PurchaseOrderId { get; set; }

        public DateOnly? ShipmentDate { get; set; }
    }
}
