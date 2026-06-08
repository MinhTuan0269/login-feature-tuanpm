using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
    /// <summary>
    /// Service responsible for generating signed JWT access tokens.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a signed JWT access token for the specified user.
        /// </summary>
        /// <param name="user">The user entity whose identity information will be embedded in the token.</param>
        /// <returns>A signed JWT string.</returns>
        public string GenerateToken(User user)
        {
            // Step 1: Build the claims array that will be embedded in the JWT payload
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Unique user identifier
                new Claim(ClaimTypes.Email, user.Email),                   // User's email address
                new Claim("role", user.Role.Name),                         // User's assigned role
                new Claim(ClaimTypes.Name, user.FullName)                  // User's full name
            };

            // Step 2: Read the secret signing key from application configuration (appsettings.json)
            var secretKey = _configuration["Jwt:Key"];

            // Step 3: Create a SymmetricSecurityKey by encoding the secret key as UTF-8 bytes
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey!)
            );

            // Step 4: Create the SigningCredentials using the HMAC-SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Read expiration (minutes) from configuration, default to 120 minutes if missing/invalid
            var expiryMinutesSetting = _configuration["Jwt:AccessTokenMinutes"];
            if (!int.TryParse(expiryMinutesSetting, out var expiryMinutes) || expiryMinutes <= 0)
            {
                expiryMinutes = 120;
            }

            // Step 5: Construct the JWT token with issuer, audience, claims, expiry, and signing credentials
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            // Step 6: Serialize the token to a compact JWT string and return it to the caller
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
