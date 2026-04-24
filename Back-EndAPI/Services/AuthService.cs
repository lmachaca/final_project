//using Microsoft.AspNetCore.Identity;
//namespace Back_EndAPI.Services
//{
//  public class AuthService
//{


//    public string? LoginSimple(string username, string password) {
//
//      var hasher = new PasswordHasher<object>();
//    var hashedPassword = hasher.HashPassword(null, password);
//
//pretend to get hashed pw from fb 

//   string dbPasswrod = "password123"; //whatever is in the db
//  var example = _dbcontext.Authorize
//     .Where(char => char.Username == username);


//     var result = hasher.VerifyHashedPassword(null, hashedPassword, dbPasswrod);

//   if (result == PassswordVerificationResult.Success) {

//         Console.WriteLine("Success!");
// }

//   return GenerateTokensimple(username)
//   }
// }
using Back_EndAPI.Data;
using Back_EndAPI.Entities;
using ClassLibrary.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Back_EndAPI.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
  

    public AuthService(AppDbContext dbContext, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // DEVELOPMENT ONLY: Test user for Swagger testing
        if (request.Username == "testuser" && request.Password == "password123")
        {
            _logger.LogWarning("⚠️ Using test user (development only)");

            var ttoken = GenerateToken(new Login { Id = 999, Username = "testuser" });
            var expiresAt = DateTime.UtcNow.AddHours(1);

            return new LoginResponseDto
            {
                UserId = 999,
                Username = "testuser",
                Token = ttoken
                
            };
        }

        // 1. Find user by username from database
        var user = await _dbContext.Logins
            .FirstOrDefaultAsync(l => l.Username == request.Username);

        if (user == null)
        {
            _logger.LogWarning($"Login failed: User '{request.Username}' not found");
            throw new ArgumentException("Invalid username or password");
        }

        // 2. Verify password (simple comparison with stored password)
        if (user.Password != request.Password)
        {
            _logger.LogWarning($"Login failed: Invalid password for user '{request.Username}'");
            throw new ArgumentException("Invalid username or password");
        }

        // 3. Generate JWT token
        var token = GenerateToken(user);
    

        _logger.LogInformation($"User '{user.Username}' logged in successfully");

        return new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username ?? string.Empty,
            Token = token
       
        };
    }

    private string GenerateToken(Login user)
    {
        var key = _configuration["JwtKey"] ?? "THIS_IS_MY_SECRET_KEY_1234567890";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("username", user.Username ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public static class PasswordHashingHelper
    {
        public static string HashPassword(string password)
        {
            var hasher = new PasswordHasher<Login>();
            return hasher.HashPassword(null, password);
        }
    }
}