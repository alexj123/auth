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
        
        public DefaultError(string message)
        {
            Message = message;
        }
    }
}