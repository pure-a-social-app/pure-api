using Pure.Common.Commands;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pure.Common.Auth
{
    public class JwtHandler : IJwtHandler
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly JwtOptions _options;
        private readonly SecurityKey _issuerSigninKey;
        private readonly SigningCredentials _signingCredentials;
        private readonly JwtHeader _jwtHeader;
        private readonly TokenValidationParameters _tokenValidationParameters;

        // TODO: ASK: Debug password
        public JwtHandler(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            _issuerSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            _signingCredentials = new SigningCredentials(_issuerSigninKey, SecurityAlgorithms.HmacSha256);
            _jwtHeader = new JwtHeader(_signingCredentials);
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidIssuer = _options.Issuer,
                IssuerSigningKey = _issuerSigninKey
            };
        }

        public JsonWebToken Create(string userId, UserRole userRole)
        {
            var nowUtc = DateTime.UtcNow;
            var expires = nowUtc.AddMinutes(_options.ExpiryMinutes);
            var centuryBegin = new DateTime(1970, 1, 1).ToUniversalTime();
            var exp = (long)(new TimeSpan(expires.Ticks - centuryBegin.Ticks).TotalSeconds);
            var iat = (long)(new TimeSpan(nowUtc.Ticks - centuryBegin.Ticks).TotalSeconds);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userId.ToString()),
                new Claim(ClaimTypes.Role, userRole.ToString())
            };

            var payload = new JwtPayload
            {
                { "userId", userId },
                { "iss", _options.Issuer },
                { "iat", iat },
                { "exp", exp },
                { "unique_name", userId },
                { "userrole", userRole }
            };

            payload.AddClaims(claims);
            var jwt = new JwtSecurityToken(_jwtHeader, payload);
            var token = _jwtSecurityTokenHandler.WriteToken(jwt);

            return new JsonWebToken
            {
                Id = userId,
                Token = token,
                Expires = exp,
                UserRole = userRole.GetUserRoleEncryptedString()
            };
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenValidationParameterWithoutLifeTime = _tokenValidationParameters;
                tokenValidationParameterWithoutLifeTime.ValidateLifetime = false;
                var principal = _jwtSecurityTokenHandler
                    .ValidateToken(token, tokenValidationParameterWithoutLifeTime, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg
                        .Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid Token");
                }

                return principal;
            }
            catch (Exception e)
            {
                return null; 
            }
        }
    }

    public static class AuthenticationUtil
    {
        public static string GetUserRoleEncryptedString(this UserRole userRole)
        {
            switch (userRole)
            {
                case UserRole.User:
                    return "fIgHfRV45XSS93NB";
                case UserRole.Retailer:
                    return "gRCzm0UI1tTxsdJH";
                default:
                    return "fIgHfRV45XSS93NB";
            }
        }
    }
}
