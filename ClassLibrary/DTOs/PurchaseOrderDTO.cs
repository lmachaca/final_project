using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class PurchaseOrderResponseDto
    {
        public int Id { get; set; }
        public DateOnly DateOrdered { get; set; }
        public int? VendorId { get; set; }
    }
}
