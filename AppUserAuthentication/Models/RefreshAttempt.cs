using System.ComponentModel.DataAnnotations;

namespace AppUserAuthentication.Models
{
    /// <summary>
    /// A class for a refresh attempt.
    /// </summary>
    public class RefreshAttempt
    {
        /// <summary>
        /// The refresh token.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// The JWT.
        /// </summary>
        [Required]
        public string Jwt { get; set; }
    }
}