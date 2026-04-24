using Back_EndAPI.Services;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back_EndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransferRecordsController : ControllerBase
    {
        private readonly ITransferRecordService _transferRecordService;
        private readonly ILogger<TransferRecordsController> _logger;

        public TransferRecordsController(ITransferRecordService transferRecordService, ILogger<TransferRecordsController> logger)
        {
            _transferRecordService = transferRecordService;
            _logger = logger;
        }

        [HttpPost("store")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StoreTransferRecordResponseDto>> StoreTransferRecord(
            [FromBody] StoreTransferRecordRequestDto request)
        {
            try
            {
                var result = await _transferRecordService.StoreTransferRecordAsync(request);
                return CreatedAtAction(nameof(StoreTransferRecord), new { id = result.TransferRecordId }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid transfer record store request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error storing transfer record: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while storing the transfer record" });
            }
        }
    }
}