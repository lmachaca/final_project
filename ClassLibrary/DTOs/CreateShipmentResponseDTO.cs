using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{

    public class CreateShipmentResponseDto
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public DateOnly? ShipmentDate { get; set; }
    }
}
