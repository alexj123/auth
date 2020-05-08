using System;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using AppUserAuthentication.Access.Actions;
using AppUserAuthentication.Models.Identity;
using AppUserAuthentication.TokenGeneration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppUserAuthentication.Access.Repositories
{
    /// <summary>
    /// Repository for the collection of <see cref="AppUser"/>.
    ///
    /// NOTE: Requires logger injection, NLog recommended.
    /// </summary>
    public class UserRepository<T> : IUserRepository<T> where T : AppUser
    {
        private readonly UserManager<T> _userManager;
        private readonly IJwtHandler _jwtHandler;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        /// <summary>
        /// Constructs a new repository.
        /// </summary>
        /// <param name="userManager">The user manager to handle IdentityUsers.</param>
        /// <param name="jwtHandler">The jwt handler to generate and refresh tokens.</param>
        /// <param name="refreshTokenGenerator">The refresh token handler to generate refresh tokens.</param>
        public UserRepository(UserManager<T> userManager, IJwtHandler jwtHandler, IRefreshTokenGenerator refreshTokenGenerator)
        {
            _userManager = userManager;
            _jwtHandler = jwtHandler;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        /// <inheritdoc />
        public async Task<IUserActionResult> Create(T user, string password)
        {
            var identityResult = await _userManager.CreateAsync(user, password);

            if (identityResult.Errors.Any())
            {
                return new DefaultUserActionResultBuilder()
                    .WithIdentityErrors(identityResult.Errors)
                    .Build();
            }
            
            //Generate tokens for the user
            var jwt = _jwtHandler.Generate(DefaultJwtHandler.GetDefaultClaims(user.FirstName, user.Email));
            var refreshToken = _refreshTokenGenerator.Generate();
        
            //Add token to db and update user
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
            
            return new DefaultUserActionResultBuilder()
                .Success()
                .WithJwt(jwt)
                .WithRefreshToken(refreshToken.Token)
                .Build();
        }

        /// <inheritdoc />
        public async Task<IUserActionResult> Authenticate(T user, string password)
        {
            var identityResult = await _userManager.CheckPasswordAsync(user, password);

            if (!identityResult)
            {
                return new DefaultUserActionResultBuilder()
                    .AddError("Invalid credentials supplied")
                    .Build();
            }
            
            //Generate tokens for the user
            var jwt = _jwtHandler.Generate(DefaultJwtHandler.GetDefaultClaims(user.FirstName, user.Email));
            var refreshToken = _refreshTokenGenerator.Generate();
            
            //Add token to db and update user
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            return new DefaultUserActionResultBuilder()
                .Success()
                .WithJwt(jwt)
                .WithRefreshToken(refreshToken.Token)
                .Build();
        }

        /// <inheritdoc />
        public async Task<T> FindByEmail(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <inheritdoc />
        public async Task<T> FindByUsername(string username)
        {
            return await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == username);
        }

        /// <summary>
        /// A list of default errors for invalid refresh token, as it's used so frequently by <see cref="RefreshToken"/>.
        /// </summary>
        private readonly IUserActionResult _defaultTokenRefreshErrors =
            new DefaultUserActionResultBuilder().AddError("Invalid token").Build();

        /// <inheritdoc cref="IUserRepository{T}.RefreshToken"/>
        /// <exception cref="ArgumentNullException">if email claim is null.</exception>
        /// <exception cref="SecurityException">if jwt is null or empty</exception>
        public async Task<IUserActionResult> RefreshToken(string jwt, string refreshToken)
        {
            //get the principal
            var principal = _jwtHandler.GetPrincipalFromExpiredToken(jwt);
            if (principal == null)
            {
                return _defaultTokenRefreshErrors;
            }

            //check if the email in the claim is null, if it is an error occurred.
            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (email == null) {
                throw new NullReferenceException("Email was null");
            }
            
            //find the user by email and their refresh tokens
            var user = await FindByEmailIncludingRefreshTokens(email);
            var storedRefreshToken = user?.RefreshTokens.FirstOrDefault(c => c.Token == refreshToken);
            //if there is no valid RefreshToken return
            if (storedRefreshToken == null || storedRefreshToken.IsExpired())
            {
                return _defaultTokenRefreshErrors;
            }

            var newJwt = _jwtHandler.Generate(DefaultJwtHandler.GetDefaultClaims(user.FirstName, user.Email));
            var newRefreshToken = _refreshTokenGenerator.Generate();

            user.RefreshTokens.Remove(storedRefreshToken);
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            return new DefaultUserActionResultBuilder()
                .Success()
                .WithJwt(newJwt)
                .WithRefreshToken(newRefreshToken.Token)
                .Build();
        }

        /// <summary>
        /// Finds a user by email and includes their refresh tokens in the result.
        /// </summary>
        /// <param name="email">the email to query for</param>
        /// <returns>A user with their refresh tokens</returns>
        public Task<T> FindByEmailIncludingRefreshTokens(string email)
        {
            return _userManager.Users.Include(c => c.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}