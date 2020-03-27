using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace AppUserAuthentication.Access.Actions
{
    /// <summary>
    /// A builder class for a <see cref="DefaultUserActionResult"/>
    /// </summary>
    public class DefaultUserActionResultBuilder
    {
        private readonly DefaultUserActionResult _defaultUserActionResult;
        
        public DefaultUserActionResultBuilder()
        {
            _defaultUserActionResult = new DefaultUserActionResult();
        }

        /// <summary>
        /// Sets the action result to succeeded.
        ///
        /// Defaults succeeded to false.
        /// </summary>
        /// <returns>this</returns>
        public DefaultUserActionResultBuilder Success()
        {
            _defaultUserActionResult.Succeeded = true;
            return this;
        }

        /// <summary>
        /// Sets the errors of the action result.
        /// </summary>
        /// <param name="errors">the errors</param>
        /// <returns>this</returns>
        public DefaultUserActionResultBuilder WithErrors(IList<DefaultError> errors)
        {
            _defaultUserActionResult.Errors = errors;
            return this;
        }

        /// <summary>
        /// Adds an error with the provided msg to list of errors.
        /// </summary>
        /// <param name="msg">the error msg to add</param>
        /// <returns>this</returns>
        public DefaultUserActionResultBuilder AddError(string msg)
        {
            _defaultUserActionResult.Errors.Add(new DefaultError(msg));
            return this;
        }
        
        /// <summary>
        /// Sets the errors using a list of identity errors.
        ///
        /// Uses the descriptions of the identity errors.
        /// </summary>
        /// <param name="errors">the errors</param>
        /// <returns>this</returns>
        public DefaultUserActionResultBuilder WithIdentityErrors(IEnumerable<IdentityError> errors)
        {
            _defaultUserActionResult.Errors = errors.Select(c => new DefaultError(c.Description)).ToList();
            return this;
        }

        /// <summary>
        /// Sets the Jwt.
        /// </summary>
        /// <param name="jwt">the jwt</param>
        /// <returns>this</returns>
        public DefaultUserActionResultBuilder WithJwt(string jwt)
        {
            _defaultUserActionResult.Jwt = jwt;
            return this;
        }
        
        /// <summary>
        /// Sets the refresh token.
        /// </summary>
        /// <param name="refreshToken">the refresh token</param>
        /// <returns>this</returns>
        public DefaultUserActionResultBuilder WithRefreshToken(string refreshToken)
        {
            _defaultUserActionResult.RefreshToken = refreshToken;
            return this;
        }

        /// <summary>
        /// Builds the action result.
        /// </summary>
        /// <returns></returns>
        public DefaultUserActionResult Build()
        {
            return _defaultUserActionResult;
        }
    }
}