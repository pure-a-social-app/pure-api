using Pure.Common.Commands;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Pure.Common.Auth
{
    public interface IJwtHandler
    {
        JsonWebToken Create(string userId, UserRole userRole);
        ClaimsPrincipal ValidateToken(string token);
    }
}
