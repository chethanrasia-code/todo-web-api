namespace TodoApi.Api.Models
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        // Optional PFX settings for RSA signing (dev/test)
        public string? PfxPath { get; set; }
        public string? PfxPassword { get; set; }
        public int ExpiryInMinutes { get; set; } = 60;
    }
}
