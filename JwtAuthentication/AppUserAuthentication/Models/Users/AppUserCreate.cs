using System.ComponentModel.DataAnnotations;

namespace AppUserAuthentication.Models.Users
{
    /// <summary>
    /// Class for creating a user.
    /// </summary>
    public class AppUserCreate : IUser
    {
        /// <summary>
        /// The username of a user.
        /// </summary>
        [Required] 
        public string UserName { get; set; }
        
        /// <summary>
        /// The email of a user.
        /// </summary>
        [Required, MaxLength(128), DataType(DataType.EmailAddress), 
         RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
        public string Email { get; set; }
        
        /// <summary>
        /// The first name of a user.
        /// </summary>
        [Required, MaxLength(128)]
        public string FirstName { get; set; }
        
        /// <summary>
        /// The last name of a user.
        /// </summary>
        [Required, MaxLength(128)]
        public string LastName { get; set; }
        
        /// <summary>
        /// The phone number of a user.
        /// </summary>
        [Required, DataType(DataType.PhoneNumber), RegularExpression(@"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$")]
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// The password of a user.
        /// </summary>
        [Required, MaxLength(256)] 
        public string Password { get; set; }
        
        /// <summary>
        /// The second password field.
        ///
        /// Should be equal to the other password field.
        /// </summary>
        [Required, Compare("Password"), MaxLength(128)]
        public string PasswordConfirm { get; set; }
    }
}