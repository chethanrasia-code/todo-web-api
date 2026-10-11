using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Logging;
using TodoApi.Application.Interfaces;

namespace TodoApi.Infrastructure.Jwt
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _secretKey;
        private readonly RsaSecurityKey _rsaKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenGenerator(string secretKey, string issuer, string audience)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
        }

        // Dev/test constructor to load an RSA private key from a PFX file on disk.
        public JwtTokenGenerator(string pfxPath, string pfxPassword, string issuer, string audience, bool loadFromPfx = true)
        {
            if (string.IsNullOrEmpty(pfxPath)) throw new ArgumentException("pfxPath is required", nameof(pfxPath));
            // Load the certificate containing the private key. Use EphemeralKeySet to avoid permission issues in dev.
            var cert = new X509Certificate2(pfxPath, pfxPassword, X509KeyStorageFlags.EphemeralKeySet);
            var rsa = cert.GetRSAPrivateKey();
            if (rsa == null) throw new ArgumentException("The provided PFX does not contain an RSA private key.", nameof(pfxPath));

            _rsaKey = new RsaSecurityKey(rsa) { KeyId = cert.Thumbprint };
            _issuer = issuer;
            _audience = audience;
        }

        public string GenerateToken(string userId, IEnumerable<Claim> claims)
        {
            SigningCredentials creds;

            if (_rsaKey != null)
            {
                // Use RS256 when RSA key is available
                creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            }
            else
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
