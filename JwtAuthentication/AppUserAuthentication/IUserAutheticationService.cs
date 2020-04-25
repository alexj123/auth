using System.Threading.Tasks;
using AppUserAuthentication.Access.Actions;
using AppUserAuthentication.Models.Users;

namespace AppUserAuthentication
{
    /// <summary>
    /// An interface for a UserService.
    /// </summary>
    public interface IUserAuthenticationService
    {
        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="userLogin">The UserLogin object.</param>
        /// <returns>A Task containing an <see cref="IUserActionResult"/>.</returns>
        Task<IUserActionResult> Authenticate(AppUserLogin userLogin);

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="userCreate">The UserCreate object.</param>
        /// <returns>A Task containing an <see cref="IUserActionResult"/>.</returns>
        Task<IUserActionResult> Create(AppUserCreate userCreate);

        /// <summary>
        /// Refreshes the Jwt using the RefreshToken.
        /// </summary>
        /// <param name="jwt">The Jwt.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>A Task containing an <see cref="IUserActionResult"/>.</returns>
        Task<IUserActionResult> Refresh(string jwt, string refreshToken);
    }
}