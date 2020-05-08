namespace AppUserAuthentication.Access.Actions
{
    /// <summary>
    /// Class that holds only the message of the error
    /// </summary>
    public class DefaultError
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Constructs this object
        /// </summary>
        /// <param name="message">The message of this error.</param>
        public DefaultError(string message)
        {
            Message = message;
        }
    }
}