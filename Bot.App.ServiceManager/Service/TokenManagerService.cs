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
        private readonly JwtTokenConfig _jwtTokenConfig;

        public TokenManagerService(IOptions<JwtTokenConfig> options)
        {
            _jwtTokenConfig = options.Value;
        }

        public async Task<string> ReadJwtToken(string authorization, string companyCode)
        {
            string userId = string.Empty;
            JwtSetting jwtSetting = await GetTokenKey(companyCode);

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
                        ValidIssuer = jwtSetting.Issuer, //_configuration["jwtSetting:Issuer"],
                        ValidAudience = jwtSetting.Issuer, //_configuration["jwtSetting:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key!))
                    }, out SecurityToken validatedToken);

                    var securityToken = handler.ReadToken(token) as JwtSecurityToken;
                    userId = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")!.Value;
                }
            }
            return userId;
        }

        private async Task<JwtSetting> GetTokenKey(string companyCode)
        {
            var values = companyCode.Split("-");
            if (values.Length < 2)
            {
                throw new ArgumentException("Invalid company code used");
            }

            var jwtSetting = await Task.FromResult(_jwtTokenConfig.GetJwtSettingsDetail(values[0].ToLower()));
            jwtSetting.CompanyCode = values[1];
            return jwtSetting;
        }

        public async Task<string> GenerateAccessToken(RequestToken requestToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var num = new Random().Next(1, 10);

            await ValidateTokenGeneratorRequest(requestToken);
            JwtSetting jwtSetting = await GetTokenKey(requestToken.CompanyCode!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                    new Claim(JwtRegisteredClaimNames.Sid, requestToken.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, requestToken.Email!),
                    new Claim(ClaimTypes.Role, requestToken.Role!),
                    new Claim(JwtRegisteredClaimNames.Aud, num.ToString()),
                    new Claim(ClaimTypes.Version, "1.0.0"),
                    new Claim(Constants.CompanyCode, jwtSetting.CompanyCode!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(Constants.JBot, JsonConvert.SerializeObject(requestToken))
                }),

                // ----------- Expiry time at after what time token will get expired -----------------------------
                Expires = DateTime.UtcNow.AddSeconds(jwtSetting.DefaulExpiryTimeInSeconds * 6),

                SigningCredentials = new SigningCredentials(
                                           new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key!)),
                                           SecurityAlgorithms.HmacSha256
                                    )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var generatedToken = tokenHandler.WriteToken(token);
            return await Task.FromResult(generatedToken);
        }

        private async Task ValidateTokenGeneratorRequest(RequestToken requestToken)
        {
            if (requestToken == null)
            {
                throw new ArgumentException("Null of invalid request object");
            }

            if (requestToken.Email == null)
            {
                throw new ArgumentException("Invalid email id passed");
            }

            if (requestToken.Role == null)
            {
                throw new ArgumentException("Invalid role used");
            }

            if (requestToken.CompanyCode == null)
            {
                throw new ArgumentException("CompanyCode must not be null");
            }

            await Task.CompletedTask;
        }
    }
}
