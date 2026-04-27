using Back_EndAPI.Services;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Back_EndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryReportService _inventoryReportService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryReportService inventoryReportService, ILogger<InventoryController> logger)
        {
            _inventoryReportService = inventoryReportService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<InventoryResponseDto>> GetInventory([FromQuery] int? productId = null)
        {
            try
            {
                if (productId.HasValue && productId.Value <= 0)
                {
                    return BadRequest(new { message = "Product ID must be greater than 0" });
                }

                var result = await _inventoryReportService.GetInventoryAsync(productId);

                _logger.LogInformation($"Inventory report requested for productId: {productId ?? 0}");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid inventory request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting inventory: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving inventory" });
            }
        }
    }
}
