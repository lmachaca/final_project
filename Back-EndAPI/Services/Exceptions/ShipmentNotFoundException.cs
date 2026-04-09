namespace Back_EndAPI.Services.Exceptions
{
    public class ShipmentAlreadyReceivedException : Exception
    {
        public ShipmentAlreadyReceivedException(int shipmentId)
            : base($"Shipment with ID {shipmentId} has already been received")
        {
        }
    }
}
