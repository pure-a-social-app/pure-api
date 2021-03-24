using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Auth;
using Pure.Common.Auth;
using Pure.Common.Commands;
using Pure.Common.Contracts;
using Pure.Common.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pure.api.Domain.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        // These constants may be changed without breaking existing hashes.
        public const int SALT_BYTES = 24;
        public const int HASH_BYTES = 18;
        public const int PBKDF2_ITERATIONS = 64000;

        private IRepository<User> _userRepository;
        private IRepository<Login> _loginRepository;

        private IPasswordStorage _encryptPassword;
        private IJwtHandler _jwtHandler;
        private IEmailService _emailService;

        private readonly string WelcomeEmailForClientTemplateId = "6830949d-444e-45f0-a6a0-c55bb3d69c26";
        private readonly string WelcomeEmailForDeveloperTemplateId = "d1a0e34b-2f8e-4a03-88c9-806d018d06b7";
        private readonly string EmailConfirmationTemplateId = "6830949d-444e-45f0-a6a0-c55bb3d69c26";
        private readonly string ResetPaswordTemplateId = "653d52e2-5df5-415c-aa0f-ffbe2da6a30a";
        private readonly string ResetPasswordConfirmationTemplateId = "d529c7be-0cc9-4385-aa9f-ad92441e7619";
        private readonly string InvitaionEmailForEmployers = "3a5bc61d-3db4-4849-afc1-6e582cfc4416";

        public AuthenticationService(IRepository<User> userRepository, 
                                IRepository<Login> loginRepository, 
                                IPasswordStorage encryptPassword, 
                                IJwtHandler jwtHandler, 
                                IEmailService emailService)
        {
            _userRepository = userRepository;
            _loginRepository = loginRepository;
            _encryptPassword = encryptPassword;
            _jwtHandler = jwtHandler;
            _emailService = emailService;
        }

        public async Task<JsonWebToken> SignUp(CreateUser user)
        {
            try
            {
                DateTime expiredOn = DateTime.UtcNow;

                var passhash = _encryptPassword.CreateHash(user.Password);
                var uId = Guid.NewGuid();
                var objectId = ObjectId.GenerateNewId().ToString();
                var login = new Login
                {
                    Id = objectId,
                    UId = uId,
                    Username = user.Email,
                    PasswordHash = passhash,
                    IsDisabled = false,
                    EmailAddressAuthorized = true,
                    ExpiredOn = expiredOn.AddYears(1),
                    PasswordFormat = PBKDF2_ITERATIONS,
                };

                var userId = Guid.NewGuid();
                var userObjectId = ObjectId.GenerateNewId().ToString();
                var userModel = new User
                {
                    Id = userObjectId,
                    UId = userId,
                    WalletAddress = user.WalletAddress,
                    UserRole = user.UserRole,
                    UserName = user.UserName,
                    Email = user.Email,
                    CreatedOn = expiredOn,
                    IsDeleted = false,
                    Login = login,
                    PostIds = new List<string>(),
                    LikedPosts = new List<string>(),
                    CommentedPosts = new List<string>(),
                };

                await _userRepository.Add(userModel);

                var jsonWebToken = _jwtHandler.Create(login.Id, user.UserRole);
                jsonWebToken.UserName = userModel.UserName;
                jsonWebToken.UserId = userModel.Id;
                jsonWebToken.Id = userModel.Login.Id;
                jsonWebToken.Expires = expiredOn.Ticks;

                return jsonWebToken;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Register error - " + ex.Message);
            }
        }

        public async Task<string> InviteUser(CreateUser user)
        {
            await SignUp(user);

            return null;
        }

        public async Task<bool> UniqueEmail(string email)
        {
            var existingUser = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();

            if (existingUser == null)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> VerifyPassword(string email, string password)
        {
            var user = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();

            if (user != null)
            {
                return await VerifyPassword(user.Login, password);
            }

            return false;
        }

        public async Task<bool> VerifyPassword(Login login, string password)
        {
            //TODO for testing purpose
            if (string.IsNullOrEmpty(password))
            {
                throw new ApplicationException("Login fail - Password is null");
            }

            if (login == null)
            {
                throw new ApplicationException("Login fail - user is null");
            }

            return _encryptPassword.VerifyPassword(password, login.PasswordHash);
        }

        public async Task ResetPassword(Login user, string newPassword)
        {
            if (user == null)
            {
                throw new ApplicationException("Incomplete reset password - user is null");
            }

            // hash password
            var passHash = _encryptPassword.CreateHash(newPassword);
            user.PasswordHash = passHash;

            await _loginRepository.Update(user);
        }

        public async Task<JsonWebToken> LoginAsync(string email, string password)
        {
            var user = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();

            if (user == null) throw new ApplicationException("User is not found");

            var userRole = user.UserRole;

            var login = new Login
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UId = Guid.NewGuid(),
                Username = user.Email,
                PasswordHash = user.Login.PasswordHash,
                IsDisabled = false,
                EmailAddressAuthorized = true,
                ExpiredOn = DateTime.UtcNow.AddYears(1),
                PasswordFormat = PBKDF2_ITERATIONS,
            };

            var userName = "";

            if (userRole == UserRole.User)
            {
                user = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();
                user.Login = login;
                userName = user.UserName;

                await _userRepository.Update(user);
            }
            else
            {
                throw new ApplicationException("User Role is not found");
            }

            var passwordCorrect = await VerifyPassword(user.Login, password);

            if (!passwordCorrect)
            {
                throw new ApplicationException("Invalid credentials");
            }
                
            var jsonWebToken = _jwtHandler.Create(user.Login.Id, userRole);
            jsonWebToken.UserName = userName; 
            jsonWebToken.UserId = user.Id;
            jsonWebToken.WalletAddress = user.WalletAddress;
            jsonWebToken.ShopAddress = user.ShopAddress != null ? user.ShopAddress : null;
            jsonWebToken.Id = user.Login.Id;
            jsonWebToken.Expires = DateTime.UtcNow.Ticks;

            return jsonWebToken;
        }

        public async Task SendWelcomeEmail(string email, string name, UserRole userRole)
        {
            var templateId = userRole == UserRole.User 
                ? WelcomeEmailForClientTemplateId : WelcomeEmailForDeveloperTemplateId;

            try
            {
                EmailMessage message = new EmailMessage
                {
                    Subject = "Welcome Email",
                    FirstName = name,
                    Destination = email
                };

                Dictionary<string, string> presetSubstitutions = new Dictionary<string, string>
                {
                    { "{%user_name%}", name } // ASK format
                };

                await _emailService.SendAsync(message, templateId, presetSubstitutions);
            }
            catch (Exception ex)
            {
                if (ex != null) // ASK: Why we need this?
                {
                    throw new ApplicationException("Email Server Error");
                }
            }
        }

        public async Task EmailConfirmationLink(string email, string firstName, string token)
        {
            try
            {
                EmailMessage message = new EmailMessage
                {
                    Subject = "Email Confirmation",
                    FirstName = firstName,
                    Destination = email
                };
                Dictionary<string, string> presetSubstitutions = new Dictionary<string, string>
                {
                    {"{%user_name%}", message.FirstName},
                    {"{%url%}",token }
                };
                await _emailService.SendAsync(message, EmailConfirmationTemplateId, presetSubstitutions);
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    throw new ApplicationException("Email Server Error");
                }
            }
        }

        public async Task<bool> IsEmailVerified(string email, UserRole userRole)
        {
            bool isVerified;

            switch (userRole)
            {
                case UserRole.User:
                    var existingClient = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();
                    isVerified = existingClient.Login.EmailAddressAuthorized;
                    return isVerified;
                default:
                    throw new ApplicationException("Invalid email");
            }
        }

        public async Task<bool> VerifyEmail(UserRole userRole, string userId, string clientEmail)
        {
            try
            {
                bool isAlreadyVerified = false;

                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null && !user.Login.EmailAddressAuthorized)
                {
                    user.Login.EmailAddressAuthorized = true;
                    await _userRepository.Update(user);
                    await SendWelcomeEmail(user.Login.Username, user.UserName, user.UserRole);
                    return user.Login.EmailAddressAuthorized;
                }

                if (isAlreadyVerified)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException("Something went wrong while verifying email address contact Administrator");
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Something went wrong while verifying email address contact Administrator");
            }
        }

        public async Task ResendEmailVerification(string email)
        {
            try
            {
                var existingUser = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();

                if (existingUser == null)
                {
                    throw new ArgumentException("Invalid email");
                }
                else if (existingUser.Login.EmailAddressAuthorized)
                {
                    throw new InvalidOperationException("Email address has already been verified");
                }
                else
                {
                    string id = "", firstname = "";
                    UserRole role = UserRole.User;
                    bool isSignUp = true;
                    if (existingUser != null) { id = existingUser.Id; role = UserRole.User; firstname = existingUser.UserName; }

                    var token = _jwtHandler.Create(id, role);

                    var tokenLink = "http://localhost:61771/Home/UserSettings?pagetype=verifyEmail&&token=" + token.Token;

                    await EmailConfirmationLink(email, firstname, tokenLink);
                }
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ApplicationException("Cannot send verification email");
            }
        }

        public async Task<bool> ForgetPassword(string email)
        {
            UserRole userRole = await DetermineUserRole(email);

            var resetEmail = email.ToLowerInvariant();
            string token = "";
            string userFirstName = "";
            string tokenExpiryDate = DateTime.UtcNow.AddDays(2).ToString();
            string userId = "";

            var user = _userRepository.GetQueryable().Where(x => x.Login.Username.Equals(email)).FirstOrDefault();
            if (user == null)
            {
                return false;
            }

            //generate token and update reset token and expiry date
            token = await GeneratePasswordResetTokenAsync(user.Id);
            user.Login.ResetPasswordToken = token;
            user.Login.ResetPasswordTokenExpiryDate = DateTime.UtcNow.AddDays(2);
            await _userRepository.Update(user);

            // ASK: When we need it?
            tokenExpiryDate = user.Login.ResetPasswordTokenExpiryDate != null 
                ? user.Login.ResetPasswordTokenExpiryDate.Value.ToString("dddd, dd MMMM yyyy h:mm tt") 
                : throw new ApplicationException("Token invalid");

            string resetUrl = "http://localhost:61771/ResetPassword/" + resetEmail + "/" + token;

            var uriBuilder = new UriBuilder(resetUrl);

            resetUrl = uriBuilder.ToString();

            EmailMessage message = new EmailMessage
            {
                Subject = "Reset Password",
                FirstName = userFirstName,
                Destination = email
            };

            Dictionary<string, string> presetSubstitutions = new Dictionary<string, string>
            {
                {"{%user_name%}", message.FirstName},
                {"{%reset_url%}", resetUrl },
                {"{%expiry_date%}", tokenExpiryDate }
            };

            try
            {
                await _emailService.SendAsync(message, ResetPaswordTemplateId, presetSubstitutions);
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    throw new ApplicationException("Email Server Error");
                }
            }

            return true; //INCOMPLETE: false parameter placeholder for rememberMe
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string id)
        {
            Random random = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(id); // ASK: Why StringBulder

            result.Append(characters[random.Next(characters.Length)]); // TODO: Debug

            return result.ToString();
        }

        public async Task ResetPassword(string email, string newPassword)
        {
            if (email == null)
            {
                throw new ApplicationException("Incomplete reset password - user is null");
            }

            UserRole userRole = await DetermineUserRole(email);
            string userName = "";

            // hash password
            var user = _userRepository.GetQueryable().Where(x => x.Login.Username.Equals(email)).FirstOrDefault();
            var passHash = _encryptPassword.CreateHash(newPassword);

            user.Login.PasswordHash = passHash;
            user.Login.ResetPasswordToken = null;
            user.Login.ResetPasswordTokenExpiryDate = null;

            await _userRepository.Update(user);
            userName = user.UserName;

            try
            {
                EmailMessage message = new EmailMessage
                {
                    Subject = "Password reset successfully",
                    FirstName = userName,
                    Destination = email,
                };
                Dictionary<string, string> presetSubstitutions = new Dictionary<string, string>
                {
                    {"{%user_name%}",message.FirstName}
                };
                await _emailService.SendAsync(message, ResetPasswordConfirmationTemplateId, presetSubstitutions);
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    throw new ApplicationException("Email Server Error");
                }
            }
        }

        public async Task<bool> VerifyToken(string email, string token)
        {
            try
            {
                UserRole userRole = await DetermineUserRole(email);
                bool isTokenValid;
                var user = _userRepository.GetQueryable()
                        .Where(x => x.Login.ResetPasswordToken.Equals(token)
                        && x.Login.Username.ToLowerInvariant() == email.ToLowerInvariant()
                        && x.Login.ResetPasswordTokenExpiryDate >= DateTime.Today
                        )
                        .FirstOrDefault();
                isTokenValid = (user != null && !(string.IsNullOrEmpty(user.Id))) ? true : false;

                return isTokenValid;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error");
            }
        }

        public async Task<string> GetAccountSetting(string userId, UserRole userRole)
        {
            //TODO: when notifications are implemented and user is able to save notification/account settings
            //this method will be used to retrieve the settings, for now it will only get user's name/company name

            var user = (await _userRepository.Get(x => x.Id == userId)).FirstOrDefault();
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            return (user.UserName);
        }

        public async Task<string> ChangeUserName(string userId, UserRole userRole, string userName)
        {
            // Verify username format.
            if (!userName.Contains(","))
            {
                throw new ArgumentException("Username for Talent is not well formed. Please try: \"firstname,lastname\"");
            }

            var user = (await _userRepository.Get(x => x.Id == userId)).FirstOrDefault();
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            user.UserName = userName;
            await _userRepository.Update(user);
            return user.UserName;
        }

        public async Task<ChangePasswordResult> ChangePassword(string userId, UserRole userRole, ChangePasswordViewModel model)
        {
            var user = (await _userRepository.Get(x => x.Id == userId)).FirstOrDefault();
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            var passwordCorrect = await VerifyPassword(user.Login, model.CurrentPassword);
            if (!passwordCorrect)
            {
                return (new ChangePasswordResult() { Success = false, Message = "Current password is incorrect" });
            }

            var passHash = _encryptPassword.CreateHash(model.NewPassword);
            user.Login.PasswordHash = passHash;
            
            await _userRepository.Update(user);

            return (new ChangePasswordResult() { Success = true, Message = "Successfully changed password" });
        }

        public async Task<bool> DeactivateAccount(string userId, UserRole userRole)
        {
            var user = (await _userRepository.Get(x => x.Id == userId)).FirstOrDefault();
            user.IsDeleted = true;

            await _userRepository.Update(user);

            return (true);
        }

        public async Task<JsonWebToken> RefreshToken(string token)
        {
            var tokenPrinciple = _jwtHandler.ValidateToken(token);

            if (tokenPrinciple != null)
            {
                var id = tokenPrinciple.Claims.First(x => x.Type == "userId");

                //TODO Current logic is only for Talent User / not for Employer or Recruiter
                var user = await _userRepository.GetByIdAsync(id.Value);
                if (user == null)
                    throw new ApplicationException("Invalid Token");

                var newToken = _jwtHandler.Create(user.Id, UserRole.User);

                return newToken;
            }

            throw new ApplicationException("Invalid Token");
        }

        public async Task<UserRole> DetermineUserRole(string email)
        {
            var user = (await _userRepository.Get(x => x.Login.Username == email)).FirstOrDefault();
            if (user == null)
            {
                return UserRole.User;
            }

            return user.UserRole;
        }

        private static Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length) // TODO: Debug
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
