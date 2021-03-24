using Pure.api.Domain.Models.Auth;
using Pure.Common.Auth;
using Pure.Common.Commands;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface IAuthenticationService
    {
        Task<JsonWebToken> SignUp(CreateUser user);
        Task<string> InviteUser(CreateUser user);   
        Task<bool> UniqueEmail(string email);
        Task<bool> VerifyPassword(string email, string password);
        Task ResetPassword(Login user, string newPassword);
        Task<JsonWebToken> LoginAsync(string email, string password);
        Task SendWelcomeEmail(string email, string name, UserRole userRole);
        Task EmailConfirmationLink(string email, string firstName, string token);
        Task<bool> IsEmailVerified(string email, UserRole userRole);
        Task<bool> VerifyEmail(UserRole userRole, string userId, string clientEmail);
        Task ResendEmailVerification(string email);
        Task<bool> ForgetPassword(string email);
        Task ResetPassword(string email, string newPassword);
        Task<bool> VerifyToken(string email, string token);
        Task<string> GetAccountSetting(string userId, UserRole userRole);
        Task<string> ChangeUserName(string userId, UserRole userRole, string userName);
        Task<ChangePasswordResult> ChangePassword(string userId, UserRole userRole, ChangePasswordViewModel model);
        Task<bool> DeactivateAccount(string userId, UserRole userRole);
        Task<JsonWebToken> RefreshToken(string token);
        string RandomString(int length);
    }
}
