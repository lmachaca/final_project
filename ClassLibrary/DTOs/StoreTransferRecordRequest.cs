using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class StoreTransferRecordRequestDto
    {
        [Required(ErrorMessage = "Received Item ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Received Item ID must be a positive number")]
        public int ReceivedItemId { get; set; }

        [Required(ErrorMessage = "Storage Location ID (Bin) is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Storage Location ID must be a positive number")]
        public int StorageLocationId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
