using AppUserAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppUserAuthentication.Persistence
{
    /// <summary>
    /// Abstract AppDbContext class which has as Identity an AppUser type
    /// </summary>
    public abstract class AbstractAppDbContext<T> : IdentityDbContext<T> where T : IdentityUser
    {
        /// <summary>
        /// The list of refresh tokens.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        /// <inheritdoc cref="IdentityDbContext{T}"/>
        protected AbstractAppDbContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}