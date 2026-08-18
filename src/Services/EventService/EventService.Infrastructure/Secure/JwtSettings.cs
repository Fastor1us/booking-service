using System.ComponentModel.DataAnnotations;

namespace EventService.Infrastructure.Secure;

public class JwtSettings
{
    public const int DefaultExpiresInMinutes = 15;

    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [Range(5, 525_600, ErrorMessage =
        "ExpiresInMinutes must be between {1} and {2} minutes")]
    public int ExpiresInMinutes { get; set; } = DefaultExpiresInMinutes;
}
