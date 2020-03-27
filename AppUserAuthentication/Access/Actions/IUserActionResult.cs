using System.Collections.Generic;
using System.Linq;

namespace AppUserAuthentication.Access.Actions
{
   
    /// <summary>
    /// Class to hold the result of a user action.    
    /// </summary>
    public interface IUserActionResult
    {
        /// <summary>
        /// Bool that holds if the Action was successful or not.
        /// </summary>
        public bool Succeeded { get; set;}
        
        /// <summary>
        /// IEnumerable that holds all errors that occurred during the action.
        /// </summary>
        public IEnumerable<DefaultError> Errors { get; set;}
        
        /// <summary>
        /// A JWT from an action.
        /// </summary>
        public string Jwt { get; set; }

        /// <summary>
        /// A Refresh token from an action.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets all errors as one string.
        /// </summary>
        /// <returns>a string of all error messages</returns>
        public string GetErrorsAsString();
    }
    
    /// <summary>
    /// A default class to hold the result of a user action.
    /// </summary>
    public class DefaultUserActionResult : IUserActionResult
    {
        /// <summary>
        /// Bool that holds if the Action was successful or not.
        /// </summary>
        public bool Succeeded { get; set; }

        private IEnumerable<DefaultError> _errors;
        /// <summary>
        /// IEnumerable that holds all errors that occurred during the action.
        /// </summary>
        public IEnumerable<DefaultError> Errors
        {
            get => _errors ?? new List<DefaultError>();
            set => _errors = value;
        }

        /// <summary>
        /// A JWT, can be null of none is returned.
        /// </summary>
        public string Jwt { get; set; }

        /// <summary>
        /// A Refresh token, can be null if none is returned.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <inheritdoc cref="IUserActionResult.GetErrorsAsString"/>
        public string GetErrorsAsString()
        {
            return $"{string.Join("; ", Errors.Select(x => $"{x.Message}"))}";
        }
    }
}