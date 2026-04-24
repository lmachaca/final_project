using Back_EndAPI.Services;
using Back_EndAPI.Services.Exceptions;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back_EndAPI.Controllers.Shipment
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ILogger<ShipmentsController> _logger;
       

        public ShipmentsController(IShipmentService shipmentService, ILogger<ShipmentsController> logger)
        {
            _shipmentService = shipmentService;
            _logger = logger;
        }

        [HttpPost("{shipmentId}/receive")]
        public async Task<ActionResult<ReceiveShipmentResponseDto>> ReceiveShipment(
            int shipmentId,
            [FromBody] ReceiveShipmentRequestDto request)
        {
            try
            {
                if (request.ShipmentId != shipmentId)
                {
                    return BadRequest(new { message = "Shipment ID in URL does not match request body" });
                }

                var result = await _shipmentService.ReceiveShipmentAsync(request);
                _logger.LogInformation($"Shipment {shipmentId} received successfully");
                return Ok(result);
            }
            catch (ShipmentAlreadyReceivedException ex)
            {
                _logger.LogWarning($"Shipment already received: {ex.Message}");
                return Conflict(new { message = ex.Message });  // ← Change to Conflict
            }

            catch (ShipmentNotFoundException ex)
            {
                _logger.LogWarning($"Shipment not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }

            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid shipment receive request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error receiving shipment: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while receiving the shipment" });
            }
        }
        [HttpPost]
        public async Task<ActionResult<CreateShipmentResponseDto>> CreateShipment(
        [FromBody] CreateShipmentRequestDto request)
        {
            try
            {
                var result = await _shipmentService.CreateShipmentAsync(request);
                return CreatedAtAction(nameof(CreateShipment), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid shipment creation request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating shipment: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the shipment" });
            }
        }
    }

}