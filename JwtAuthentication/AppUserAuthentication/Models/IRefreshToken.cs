using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppUserAuthentication.Models
{
    /// <summary>
    /// Interface for a RefreshToken.
    /// </summary>
    public interface IRefreshToken
    {
        /// <summary>
        /// The Id of the RefreshToken, used in the db
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The actual refresh token.
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// The expiration date of the token.
        /// </summary>
        public long Expiration { get; set; }

        /// <summary>
        /// Checks whether this token is expired.
        /// </summary>
        /// <returns>bool whether this token is expired or not</returns>
        public bool IsExpired();
    }
    
    /// <summary>
    /// A refresh token object.
    ///
    /// Used to refresh JWTs.
    /// </summary>
    public class RefreshToken : IRefreshToken
    {
        /// <summary>
        /// The id of the RefreshToken.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        
        /// <summary>
        /// The actual refresh token.
        /// </summary>
        [Required]
        public string Token { get; set; }
        
        /// <summary>
        /// The expiration date of the token.
        /// </summary>
        [Required]
        public long Expiration { get; set; }

        /// <inheritdoc cref="IRefreshToken.IsExpired"/>
        public bool IsExpired()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() >= Expiration;
        }
    }
}