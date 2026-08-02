using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace BookingApi.Infrastructure.Security;

public class JwtTokenGenerator(
    string secret,
    string issuer,
    string audience) : ITokenGenerator
{
    public string Generate(string login, UserRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
