using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pure.Common.Commands
{
    public class LoginCommand : ICommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public LoginCommand(string email)
        {
            Email = string.IsNullOrEmpty(email) ? "" : email.ToLower();
        }
    }
}
