using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public DateTime? TokenExpireDate { get; set; }
    }

    public class VerifyResetToken
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
