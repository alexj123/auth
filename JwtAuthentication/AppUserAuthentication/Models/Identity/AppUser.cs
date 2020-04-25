using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppUserAuthentication.Models.Identity
{
    /// <summary>
    /// A class for handling users in the application.
    ///
    /// Extends <see cref="IdentityUser"/>.
    /// </summary>
    public abstract class AppUser : IdentityUser
    {
        /// <summary>
        /// The first name of a user.
        /// </summary>
        [Required, MaxLength(128)] 
        public string FirstName { get; set; }
        /// <summary>
        /// The last name of a user.
        /// </summary>
        [Required, MaxLength(128)] 
        public string LastName { get; set; }

        /// <summary>
        /// List of refresh tokens belonging to the user.
        /// </summary>
        public IList<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}