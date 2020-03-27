using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace AppUserAuthentication.TokenGeneration
{
    /// <summary>
    /// Interface for a JWT generator.
    /// </summary>
    public interface IJwtHandler
    {
        /// <summary>
        /// Generates a JWT.
        /// </summary>
        /// <param name="claims">list of claims to add to the token descriptors</param>
        /// <returns></returns>
        string Generate(List<Claim> claims);
        
        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> from a jwt.
        /// </summary>
        /// <param name="token">the token to get the principal from</param>
        /// <returns>A <see cref="ClaimsPrincipal"/></returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    /// <summary>
    /// A default implementation of a IJwtGenerator.
    /// </summary>
    public class DefaultJwtHandler : IJwtHandler
    {
        private readonly IConfiguration _config;
        private const int ExpMinutes = 30;
        
        public DefaultJwtHandler(IConfiguration config)
        {
            _config = config;
        }

        /// <inheritdoc cref="IJwtHandler.Generate"/>
        public string Generate(List<Claim> claims = null)
        {
            //Set up the claims
            if (claims == null) claims = new List<Claim>();
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddMinutes(ExpMinutes)).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
            });
            
            //Create the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(ExpMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Issuer"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var writtenToken = tokenHandler.WriteToken(token);
            
            return writtenToken;
        }
        
        /// <inheritdoc cref="IJwtHandler.GetPrincipalFromExpiredToken"/>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            //if invalid token is supplied, return null
            if (string.IsNullOrEmpty(token)) throw new SecurityException("Invalid refresh token");
            
            //setup a token            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Issuer"],
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:key"])),
                ValidateLifetime = false //we want to get expired tokens
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            
            //get the principal belonging to the token
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityException("Invalid refresh token");

            return principal;
        }

        /// <summary>
        /// Gets the default claims.
        ///
        /// These are firstname and email.
        /// </summary>
        /// <param name="firstName">the firstname</param>
        /// <param name="email">the email</param>
        /// <returns>a list of claims</returns>
        public static List<Claim> GetDefaultClaims(string firstName, string email)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, firstName)
            };
        }
    }
}