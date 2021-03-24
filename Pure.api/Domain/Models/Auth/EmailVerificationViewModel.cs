using System.ComponentModel.DataAnnotations;

namespace Pure.api.Domain.Models.Auth
{
    public class EmailVerificationViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
