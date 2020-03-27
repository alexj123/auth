namespace AppUserAuthentication.Models.Users
{
    /// <summary>
    /// An interface to set default fields for a user.
    /// </summary>
    internal interface IUser
    {
        /// <summary>
        /// The email of a user.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// The password of a user.
        /// </summary>
        public string Password { get; set; }
    }
}