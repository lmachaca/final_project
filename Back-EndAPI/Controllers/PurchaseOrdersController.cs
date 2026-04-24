using Back_EndAPI.Services;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Back_EndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ILogger<PurchaseOrdersController> _logger;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ILogger<PurchaseOrdersController> logger)
        {
            _purchaseOrderService = purchaseOrderService;
            _logger = logger;
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PurchaseOrderResponseDto>> CreatePurchaseOrder(
        CreatePurchaseOrderRequestDto request)
        {
            try
            {
                var result = await _purchaseOrderService.CreatePurchaseOrderAsync(request);
                return CreatedAtAction(nameof(CreatePurchaseOrder), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid purchase order creation request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating purchase order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the purchase order" });
            }
        }
    }
}