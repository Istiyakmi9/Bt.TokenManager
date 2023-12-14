using Bot.App.ServiceManager.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bot.App.ServiceManager.Service
{
    public class TokenManagerService
    {
        private readonly JwtSetting _jwtSetting;

        public TokenManagerService(IOptions<JwtSetting> options)
        {
            _jwtSetting = options.Value;
        }

        public string ReadJwtToken(string authorization)
        {
            string userId = string.Empty;
            if (!string.IsNullOrEmpty(authorization))
            {
                string token = authorization.Replace("Bearer", "").Trim();
                if (!string.IsNullOrEmpty(token) && token != "null")
                {
                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _jwtSetting.Issuer, //_configuration["jwtSetting:Issuer"],
                        ValidAudience = _jwtSetting.Issuer, //_configuration["jwtSetting:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key!))
                    }, out SecurityToken validatedToken);

                    var securityToken = handler.ReadToken(token) as JwtSecurityToken;
                    userId = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")!.Value;
                }
            }
            return userId;
        }

        public async Task<string> GenerateAccessToken(RequestToken requestToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var num = new Random().Next(1, 10);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                    new Claim(JwtRegisteredClaimNames.Sid, requestToken.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, requestToken.Email!),
                    new Claim(ClaimTypes.Role, requestToken.Role!),
                    new Claim(JwtRegisteredClaimNames.Aud, num.ToString()),
                    new Claim(ClaimTypes.Version, "1.0.0"),
                    new Claim(Constants.CompanyCode, requestToken.CompanyCode!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(Constants.JBot, JsonConvert.SerializeObject(requestToken))
                }),

                // ----------- Expiry time at after what time token will get expired -----------------------------
                Expires = DateTime.UtcNow.AddSeconds(_jwtSetting.DefaulExpiryTimeInSeconds * 6),

                SigningCredentials = new SigningCredentials(
                                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key!)),
                                            SecurityAlgorithms.HmacSha256
                                     )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var generatedToken = tokenHandler.WriteToken(token);
            return await Task.FromResult(generatedToken);
        }
    }
}
