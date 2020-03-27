﻿using System;
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
        private readonly ILogger<UserRepository<T>> _logger;
        private readonly IJwtHandler _jwtHandler;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public UserRepository(UserManager<T> userManager, IJwtHandler jwtHandler, IRefreshTokenGenerator refreshTokenGenerator, ILogger<UserRepository<T>> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _jwtHandler = jwtHandler;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        /// <inheritdoc cref="IUserRepository{T}.Create"/>
        public async Task<IUserActionResult> Create(T user, string password)
        {
            var identityResult = await _userManager.CreateAsync(user, password);

            if (identityResult.Errors.Any())
            {
                var result = new DefaultUserActionResultBuilder()
                    .WithIdentityErrors(identityResult.Errors)
                    .Build();
                
                _logger.LogError($"Errors creating ApplicationUser for: {user.Email} with errors: " + result.GetErrorsAsString());
                return result;
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

        /// <inheritdoc cref="IUserRepository{T}.Authenticate"/>
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

        /// <inheritdoc cref="IUserRepository{T}.FindByEmail"/>
        public async Task<T> FindByEmail(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <summary>
        /// A list of default errors for invalid refresh token, as it's used so frequently by <see cref="RefreshToken"/>.
        /// </summary>
        private readonly IUserActionResult _defaultTokenRefreshErrors =
            new DefaultUserActionResultBuilder().AddError("Invalid token").Build();

        /// <inheritdoc cref="IUserRepository{T}.RefreshToken"/>
        public async Task<IUserActionResult> RefreshToken(string jwt, string refreshToken)
        {
            ClaimsPrincipal principal;
            try
            {
                //get the principal
                principal = _jwtHandler.GetPrincipalFromExpiredToken(jwt);
            }
            catch (SecurityException)
            {
                return _defaultTokenRefreshErrors; 
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while getting the principal " + 
                                    $"for token: {jwt} and refresh token: {refreshToken}");
                return _defaultTokenRefreshErrors; 
            }
            
            //check if the email in the claim is null, if it is an error occurred.
            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (email == null) {
                _logger.LogError("Email claim was null, claims: " +
                                 $"{string.Join(", Error: ", principal.Claims.Select(x => $"Type: {x.Type}, Value: {x.Value}"))}");
                return _defaultTokenRefreshErrors;
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
                .WithJwt(jwt)
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