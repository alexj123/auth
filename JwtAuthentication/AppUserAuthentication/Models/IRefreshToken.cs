using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppUserAuthentication.Models
{
    /// <summary>
    /// A refresh token object.
    ///
    /// Used to refresh JWTs.
    /// </summary>
    public class RefreshToken
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

        /// <summary>
        /// Checks whether this token is expired.
        /// </summary>
        /// <returns>A bool.</returns>
        public bool IsExpired()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() >= Expiration;
        }
    }
}