using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dbets.Domain.Aggregates;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dbets.Infrastructure.Services;


public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    DateTime GetTokenExpiration(string token);
}