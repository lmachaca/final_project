using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
