using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pure.Common.Commands
{
    public enum UserRole
    {
        User,
        Retailer
    }

    public class CreateUser
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string WalletAddress { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid inputs.")]
        public UserRole UserRole { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "The Email field does not contain a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The First Name does not contain a valid email address.")]
        public string UserName { get; set; }
        
        public CreateUser(string email)
        {
            Email = string.IsNullOrEmpty(email) ? "" : email.ToLower();
            UserRole = UserRole.User;
        }
    }
}
