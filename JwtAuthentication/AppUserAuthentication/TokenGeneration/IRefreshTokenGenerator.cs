using System;
using System.Security.Cryptography;
using AppUserAuthentication.Models;

namespace AppUserAuthentication.TokenGeneration
{
    /// <summary>
    /// Interface for generating a refresh token.
    /// </summary>
    public interface IRefreshTokenGenerator
    {
        /// <summary>
        /// Generates a refresh token.
        /// </summary>
        /// <returns></returns>
        RefreshToken Generate();
    }
    
    /// <summary>
    /// Default implementation of <see cref="IRefreshTokenGenerator"/>.
    ///
    /// Uses <see cref="RefreshToken"/> as refresh token.
    /// </summary>
    public class DefaultRefreshTokenGenerator : IRefreshTokenGenerator
    {
        private const int ExpDays = 5;
        
        /// <inheritdoc cref="IRefreshTokenGenerator.Generate"/>
        public RefreshToken Generate()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expiration = new DateTimeOffset(DateTime.Now.AddDays(ExpDays)).ToUnixTimeSeconds()
            };
        }
    } 
}