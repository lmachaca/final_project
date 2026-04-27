using Back_EndAPI.Services;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Back_EndAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersReportController : ControllerBase
    {
        private readonly IOrderReportService _orderReportService;
        private readonly ILogger<OrdersReportController> _logger;

        public OrdersReportController(IOrderReportService orderReportService, ILogger<OrdersReportController> logger)
        {
            _orderReportService = orderReportService;
            _logger = logger;
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDetailResponseDto>> GetOrderById(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new { message = "Order ID must be greater than 0" });
                }

                var result = await _orderReportService.GetOrderByIdAsync(orderId);

                _logger.LogInformation($"Order {orderId} details retrieved");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Order not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting order details: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving order details" });
            }
        }
    }
}


