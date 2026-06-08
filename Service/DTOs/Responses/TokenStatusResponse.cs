using System;

namespace Services.DTOs.Responses
{
    public class TokenStatusResponse
    {
        public DateTime AccessTokenExpiresAt { get; set; }
        public double AccessTokenRemainingSeconds { get; set; }
        
        public DateTime RefreshTokenExpiresAt { get; set; }
        public double RefreshTokenRemainingSeconds { get; set; }
    }
}
