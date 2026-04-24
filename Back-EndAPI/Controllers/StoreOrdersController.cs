using Back_EndAPI.Services;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back_EndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoreOrdersController : ControllerBase
    {
        private readonly IStoreOrderService _storeOrderService;
        private readonly ILogger<StoreOrdersController> _logger;

        public StoreOrdersController(IStoreOrderService storeOrderService, ILogger<StoreOrdersController> logger)
        {
            _storeOrderService = storeOrderService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StoreOrderResponseDto>> CreateOrder(
            [FromBody] CreateStoreOrderRequestDto request)
        {
            try
            {
                var result = await _storeOrderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(CreateOrder), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid order creation request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the order" });
            }
        }

        [HttpPost("{orderId}/pick")]
        public async Task<ActionResult<StoreOrderResponseDto>> PickOrder(int orderId)
        {
            try
            {
                var result = await _storeOrderService.PickOrderAsync(orderId);
                _logger.LogInformation($"Order {orderId} picked successfully");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid pick request: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error picking order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while picking the order" });
            }
        }

        [HttpPost("{orderId}/pack")]

        public async Task<ActionResult<StoreOrderResponseDto>> PackOrder(int orderId)
        {
            try
            {
                var result = await _storeOrderService.PackOrderAsync(orderId);
                _logger.LogInformation($"Order {orderId} packed successfully");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid pack request: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error packing order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while packing the order" });
            }
        }

        [HttpPost("{orderId}/ship")]
        public async Task<ActionResult<StoreOrderResponseDto>> ShipOrder(int orderId)
        {
            try
            {
                var result = await _storeOrderService.ShipOrderAsync(orderId);
                _logger.LogInformation($"Order {orderId} shipped successfully");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid ship request: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error shipping order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while shipping the order" });
            }
        }
    }
}


