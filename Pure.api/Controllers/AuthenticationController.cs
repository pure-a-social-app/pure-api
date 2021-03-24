using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Auth;
using Pure.Common.Auth;
using Pure.Common.Commands;
using Pure.Common.Contracts;
using Pure.Common.Models;
using Pure.Common.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserAppContext _userAppContext;

        public AuthenticationController(
              IAuthenticationService authenticationService,
              IUserAppContext userAppContext)
        {
            _authenticationService = authenticationService;
            _userAppContext = userAppContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }

        /// <summary>
        /// Create a new account
        /// </summary>
        /// <remarks>
        /// Creates an new account for user
        /// </remarks>
        /// <param name="command">CreateUser Model</param>
        /// <response code="201">Successful. Redirects to home page with successful message</response>
        /// <response code="400">BadRequest. User input model is invalid or Email is already registered</response>   
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] CreateUser command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = ModelState.Values
                                      .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                    });
                }

                command.Email = command.Email.Trim();
                command.UserName = command.UserName.Trim();

                bool isUniqueEmail = await _authenticationService.UniqueEmail(command.Email);
                if (!isUniqueEmail)
                {
                    return BadRequest(new { Message = "This email address is already in use by another account." });
                }

                var authenticatedToken = new JsonWebToken();
                authenticatedToken = await _authenticationService.SignUp(command);

                return Ok(new { Success = true, Token = authenticatedToken });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { e.Message });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = "Please contact the IT Department for futher information" });
            }
        }

        /// <summary>
        /// Allows registered users to sign in 
        /// </summary>
        /// <remarks>
        /// Allows registered users to sign in by entering their existing username and password
        /// </remarks>
        /// <param name="command">LoginCommand Model</param>
        /// <response code="200">Successful. User's credentials are valid</response>
        /// <response code="400">BadRequest. User input model is invalid</response> 
        /// <response code="401">Unauthorized. User's credentials are invalid</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] LoginCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values
                        .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                command.Email = command.Email.Trim();
                var authenticateUser = await _authenticationService.LoginAsync(command.Email, command.Password);

                return Ok(new { isEmailVerified = true, Token = authenticateUser });
            }
            catch (ApplicationException e)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Sends account Verification Link to registered user email 
        /// </summary>
        /// <remarks>
        /// Allows a user to obtain an Verification Link sent into registered email for account activation.
        /// </remarks>
        /// <param name="vm">EmailVerificationViewModel</param>
        /// <response code="200">Successful. User email is valid and verification link is sent</response>
        /// <response code="400">BadRequest. User email is not entered or email address has been verified before</response> 
        /// <response code="401">Unauthorized. User email is invalid or System can not send verification link</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [HttpPost("accounts/email/verification")]
        public async Task<IActionResult> SendVerificationLink([FromBody] EmailVerificationViewModel vm)
        {
            try
            {
                if (vm.Email == null)
                {
                    return BadRequest(new { Message = "Please email field cannot be null!" });
                }
                await _authenticationService.ResendEmailVerification(vm.Email);
                return Ok(new { Message = "Please activate your account by clicking on the link that was sent to your email. You cannot log in before activating your account." });
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (ApplicationException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Verifies user account Verification Link via Json Web Token(JWT) 
        /// </summary>
        /// <remarks>
        /// Verifies user email account via Json Web Token(JWT) once the account Verification Link in user email has been clicked. 
        /// </remarks>
        /// <response code="200">Successful. User email is verified and user currentRole will be returned</response>
        /// <response code="400">BadRequest. User email is already verified or System Error</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("accounts/email/verification")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> VerifyEmail()
        {
            try
            {
                bool alreadyVerified = await _authenticationService.VerifyEmail(GetCurrentUserRole(), _userAppContext.CurrentUserId, "");
                if (!alreadyVerified)
                {
                    return BadRequest(new { Message = "Account already verified" });
                }

                return Ok(new { _userAppContext.CurrentRole });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { e.Message });
            }

        }

        /// <summary>
        /// Sends a password recovery email to user
        /// </summary>
        /// <remarks>
        /// This method sends a password recovery email to user's registered email address
        /// </remarks>
        /// <param name="model">ForgotPasswordViewModel</param>
        /// <response code="202">Successful. Password recovery email is sent</response>
        /// <response code="400">BadRequest. User account does not exist or Email is invalid or Email server error</response> 
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("accounts/forget_password")]
        public async Task<ActionResult> SendForgetPasswordEmail([FromBody] ForgotPasswordViewModel model)
        {
            try
            {
                var authenticateUser = await _authenticationService.ForgetPassword(model.Email);
                if (!authenticateUser)
                {
                    return BadRequest(new { Message = "Account does not exist" });
                }
                return Accepted(new
                {
                    model.Email,
                    Message = "Check your Email to reset your password"
                });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { e.Message });
            }
        }

        /// <summary>
        /// Resets authenticated user's password
        /// </summary>
        /// <remarks>
        /// Resets authenticated user's password via password reset link inside password recovery email. This methods accepts an user email, a token and a ChangePassword object as arguments. Once the validation succeeds, this method will update user password.
        /// </remarks>
        /// <param name="o">Email</param>
        /// <param name="p">Token</param>
        /// <param name="vm">ChangePasswordViewModel</param>
        /// <response code="200">Successful. Password has been reset</response>
        /// <response code="400">BadRequest. User Email is invalid or Token is invalid or Token is expired or System Error</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPut("accounts/reset_password")]
        public async Task<ActionResult> ResetPassword([FromQuery] string o, [FromQuery] string p, [FromBody] ChangePasswordViewModel vm)
        {
            try
            {
                string email = o != null ? o : throw new ArgumentException("Invalid Email");
                string token = p != null ? p : throw new ArgumentException("Invalid Token");
                string newUserPassword = vm.NewPassword != null ? vm.NewPassword : throw new ArgumentException("Invalid Password");
                bool isTokenValid = await _authenticationService.VerifyToken(o, p);
                if (isTokenValid)
                {
                    await _authenticationService.ResetPassword(o, newUserPassword);
                    return Ok();
                }
                else
                {
                    return BadRequest(new { Message = "Token is invalid or has expired" });
                }
            }
            catch (Exception e)
            {
                if (e is ApplicationException || e is ArgumentException)
                {
                    return BadRequest(new
                    {
                        e.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        e.Message
                    });
                }
            }
        }

        /// <summary>
        /// Verifies Reset Password Token
        /// </summary>
        /// <remarks>
        /// Verifies Reset Password Token sent via password reset link. 
        /// </remarks>
        /// <param name="o">Email</param>
        /// <param name="p">Token</param>
        /// <response code="200">Successful. Token is valid</response>
        /// <response code="400">BadRequest. Token is invalid or System Error</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("accounts/password/token")]
        public async Task<ActionResult> VerifyResetPasswordToken([FromQuery] string o, [FromQuery] string p)
        {
            try
            {
                bool isTokenValid = await _authenticationService.VerifyToken(o, p);
                if (isTokenValid)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch
            {
                return BadRequest("Please contact admin");
            }
        }

        /// <summary>
        /// Gets registered user account setting information
        /// </summary>
        /// <remarks>
        /// Gets registered user account setting information
        /// </remarks>
        /// <response code="200">Successful. Returns userRole, userId and userName</response>
        /// <response code="400">BadRequest. User not found</response> 
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("accounts/settings/info")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAccountSettingInfo()
        {
            var userRole = GetCurrentUserRole();
            var userId = _userAppContext.CurrentUserId;
            var userName = _authenticationService.GetAccountSetting(userId, userRole);
            return Ok(new { userRole, userId, userName });
        }

        /// <summary>
        /// Updates registered account username 
        /// </summary>
        /// <remarks>
        /// Updates registered account username.
        /// For Talent accounts the username must follow the format: { "username": "firstname,lastname" }. This format is not required for Employer or Recruiter accounts.
        /// </remarks>
        /// <param name="vm">UpdateUserInfoViewModel</param>
        /// <response code="204">NoContent. Username has been updated</response>
        /// <response code="400">BadRequest. Input is invalid or User not found</response> 
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPut("accounts/settings/username")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeUserName([FromBody] UpdateUserInfoViewModel vm)
        {
            try
            {
                if (string.IsNullOrEmpty(vm.Username))
                {
                    return BadRequest();
                }

                var userRole = GetCurrentUserRole();

                var userId = _userAppContext.CurrentUserId;
                var newName = await _authenticationService.ChangeUserName(userId, userRole, vm.Username);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return BadRequest("Request is malformed, please check the documentation");
            }
            catch (Exception)
            {

                return BadRequest();
            }

        }

        /// <summary>
        /// Updates registered account password 
        /// </summary>
        /// <remarks>
        /// Updates registered account password
        /// </remarks>
        /// <param name="model">ChangePasswordViewModel</param>
        /// <response code="200">Successful. Password has been updated</response>
        /// <response code="400">BadRequest. Credentials are invalid or User not found or Passwords not match</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("accounts/settings/password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                var isNotSame = string.Compare(model.NewPassword, model.ConfirmPassword);
                if (isNotSame != 0) return BadRequest(new { message = "Password are not matched" });
                var userRole = GetCurrentUserRole();
                var userId = _userAppContext.CurrentUserId;

                var newPassword = await _authenticationService.ChangePassword(userId, userRole, model);
                if (newPassword.Success)
                {
                    return Ok();
                }

                return BadRequest(new { Message = "Invalid credentials" });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = string.Format("Error: {0}", e.Message) });
            }

        }

        /// <summary>
        /// Deactivates registered account
        /// </summary>
        /// <remarks>
        /// Deactivates registered account
        /// </remarks>
        /// <response code="200">Successful. Account has been deactivated</response>
        /// <response code="400">BadRequest. User role is invalid</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [HttpPost("accounts/settings/deactivate")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userRole = GetCurrentUserRole();
            var userId = _userAppContext.CurrentUserId;
            try
            {
                var deactivate = await _authenticationService.DeactivateAccount(userId, userRole);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Generates a new token
        /// </summary>
        /// <remarks>
        /// Generates a new token to replace the old token
        /// </remarks>
        /// <param name="token">Token string from query</param>
        /// <response code="200">Successful. New token has been generated</response>
        /// <response code="400">BadRequest. Old token is invalid</response> 
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromQuery] string token)
        {
            try
            {
                var newToken = await _authenticationService.RefreshToken(token);

                return Ok(newToken);
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Verifies user authentication status
        /// </summary>
        /// <remarks>
        /// Verifies user authentication status: authorized or not
        /// </remarks>
        /// <returns>Content: authorized or unauthorized
        /// </returns>
        [HttpGet("authentication")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Authentication()
        {
            var userId = _userAppContext.CurrentUserId;
            return Content(string.IsNullOrEmpty(userId) ? "authorized" : "unauthorized");
        }

        #region Helpers

        private UserRole GetCurrentUserRole()
        {
            return (UserRole)Enum.Parse(typeof(UserRole), _userAppContext.CurrentRole);
        }

        #endregion
    }
}
