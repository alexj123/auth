using System.Threading.Tasks;
using AppUserAuthentication.Access.Actions;
using AppUserAuthentication.Models.Identity;

namespace AppUserAuthentication.Access.Repositories
{
    /// <summary>
    /// Interface for an AppUserRepository.
    ///
    /// </summary>
    public interface IUserRepository<T> where T : AppUser
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The <see cref="AppUser"/> to create</param>
        /// <param name="password">The password of the user</param>
        /// <returns>A <see cref="IUserActionResult"/></returns>
        public Task<IUserActionResult> Create(T user, string password);

        /// <summary>
        /// Authenticates a user.
        /// </summary>
        /// <param name="user">The user to authenticate</param>
        /// <param name="password">The password of the user</param>
        /// <returns>A <see cref="IUserActionResult"/></returns>
        public Task<IUserActionResult> Authenticate(T user, string password);

        /// <summary>
        /// Finds a user by email.
        /// </summary>
        /// <param name="email">the email</param>
        /// <returns>A user</returns>
        public Task<T> FindByEmail(string email);

        /// <summary>
        /// Refreshes the jwt using the RefreshToken.
        /// </summary>
        /// <param name="jwt">the expired jwt</param>
        /// <param name="refreshToken">the refresh token</param>
        /// <returns>A <see cref="IUserActionResult"/></returns>
        public Task<IUserActionResult> RefreshToken(string jwt, string refreshToken);
    }
}