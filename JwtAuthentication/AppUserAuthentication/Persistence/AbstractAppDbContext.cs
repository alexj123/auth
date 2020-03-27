using AppUserAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppUserAuthentication.Persistence
{
    /// <summary>
    /// Abstract AppDbContext class which has as Identity an AppUser type
    /// </summary>
    public abstract class AbstractAppDbContext<T, TF> : IdentityDbContext<T> where T : IdentityUser where TF : class, IRefreshToken
    {
        /// <summary>
        /// The list of refresh tokens.
        /// </summary>
        public DbSet<TF> RefreshTokens { get; set; }
        
        /// <inheritdoc cref="IdentityDbContext{T}"/>
        protected AbstractAppDbContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}