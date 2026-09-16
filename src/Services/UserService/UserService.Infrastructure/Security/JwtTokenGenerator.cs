using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Security.Jwt;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Security;

public class JwtTokenGenerator(IOptions<JwtSettings> jwt) : ITokenGenerator
{
    public string Generate(Guid userId, UserRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt.Value.SigningKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwt.Value.Issuer,
            jwt.Value.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.Value.ExpiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
