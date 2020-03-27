using System.Threading.Tasks;
using AppUserAuthentication.Access.Actions;
using AppUserAuthentication.Models.Users;

namespace AppUserAuthentication
{
    /// <summary>
    /// An interface of a UserService.
    ///
    /// This service: authenticates, creates a user.
    /// </summary>
    public interface IUserAuthenticationService<in T, in TF> where T : AbstractAppUserLogin where TF : AbstractAppUserCreate
    {
        /// <summary>
        /// Authenticates the user and returns a <see cref="IUserActionResult"/>.
        /// 
        /// </summary>
        /// <param name="userLogin">the user login supplied</param>
        /// <returns>Action result, containing JWT and RefreshToken</returns>
        Task<IUserActionResult> Authenticate(T userLogin);

        /// <summary>
        /// Creates the user and returns a <see cref="IUserActionResult"/>.
        /// 
        /// </summary>
        /// <param name="userCreate">the user create details supplied</param>
        /// <returns>Action result, containing JWT and RefreshToken</returns>
        Task<IUserActionResult> Create(TF userCreate);

        /// <summary>
        /// Refreshes the JWT using the RefreshToken and returns a <see cref="IUserActionResult"/>.
        /// </summary>
        /// <param name="jwt">the jwt</param>
        /// <param name="refreshToken">the refresh token</param>
        /// <returns>Action result, containing new JWT and new RefreshToken</returns>
        Task<IUserActionResult> Refresh(string jwt, string refreshToken);
    }
}