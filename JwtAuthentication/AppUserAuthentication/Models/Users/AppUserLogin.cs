using System.ComponentModel.DataAnnotations;

namespace AppUserAuthentication.Models.Users
{
    /// <summary>
    /// An abstract class to log a user in.
    ///
    /// This should be extended and used when attempting to log in.
    /// </summary>
    public class AppUserLogin : IUser
    {
        /// <summary>
        /// The email of a user.
        /// </summary>
        [Required, MaxLength(128), DataType(DataType.EmailAddress), 
         RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
        public string Email { get; set; }
        
        /// <summary>
        /// The password of a user.
        /// </summary>
        [Required, MaxLength(256)]
        public string Password { get; set; }
    }
}