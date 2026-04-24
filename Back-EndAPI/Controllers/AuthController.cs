////using Back_EndAPI.Services;
//using ClassLibrary.DTOs.Auth;
//using Microsoft.AspNetCore.Mvc;

//namespace Back_EndAPI.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class AuthController : ControllerBase
//{
//    private readonly AuthService _authService;

//    public AuthController(AuthService authService)
//    {
//        _authService = authService;
//    }

//    [HttpPost("login")]
//    public IActionResult Login([FromBody] LoginRequest request)
//    {
//        var token = _authService.Login(request.Username, request.Password);

//        if (token == null)
//            return Unauthorized();

//        return Ok(new { token });
//    }
//}

using Back_EndAPI.Services;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Back_EndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            _logger.LogInformation($"User {request.Username} logged in successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Login failed: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred during login" });
        }
    }
}