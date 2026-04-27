using Back_EndAPI.Data;
using Back_EndAPI.Entities;
using ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Services
{
    public interface IOrderReportService
    {
        Task<OrderDetailResponseDto> GetOrderByIdAsync(int orderId);
    }

    public class OrderReportService : IOrderReportService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<OrderReportService> _logger;

        public OrderReportService(AppDbContext dbContext, ILogger<OrderReportService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<OrderDetailResponseDto> GetOrderByIdAsync(int orderId)
        {
            try
            {
                // Get order with all related data
                var order = await _dbContext.CustomerOrders
                    .Include(co => co.Customer)
                    .Include(co => co.Carrier)
                    .FirstOrDefaultAsync(co => co.Id == orderId);

                if (order == null)
                {
                    throw new ArgumentException($"Order with ID {orderId} not found");
                }

                // Get order items
                var soldItems = await _dbContext.SoldItems
                    .Where(si => si.CustomerOrderId == orderId)
                    .Include(si => si.SkuNumber)
                    .ToListAsync();

                var response = new OrderDetailResponseDto
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer?.Name,
                    DateOrdered = order.DateTimeOrdered,
                    CurrentStatus = DetermineOrderStatus(orderId, soldItems),
                    Items = new List<OrderItemDetailDto>()
                };

          
                // Add shipping info if shipped
                if (response.CurrentStatus == "SHIPPED" && order.Carrier != null)
                {
                    response.ShippingInfo = new ShippingInfoDto
                    {
                        CarrierId = order.CarrierId,
                        CarrierName = order.Carrier.Name,
                        ShippingFee = order.CustomerShippingFee,
                        ShippedDate = DateTime.UtcNow, // Would come from actual shipment tracking
                        TrackingStatus = "In Transit"
                    };
                }

                _logger.LogInformation($"Order {orderId} report generated with status: {response.CurrentStatus}");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating order report: {ex.Message}");
                throw;
            }
        }
        private string DetermineOrderStatus(int orderId, List<SoldItem> items)
        {
            if (!items.Any())
                return "CREATED";

            // Check if all items have been picked (withdrawal records exist)
            // This is a simplified check - in production, you might have a status column

            // For now, return CREATED as default
            // In a real system, track this in the database
            return "CREATED";
        }

        private string DetermineItemStatus(int orderId, int itemId)
        {
            // Simplified status determination
            // In production, track this explicitly in database
            return "CREATED";
        }
    }

}

