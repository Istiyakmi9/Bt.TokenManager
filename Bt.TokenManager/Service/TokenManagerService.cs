using Bt.Lib.Common.Service.Configserver;
using Bt.Lib.Common.Service.Model;
using Bt.TokenManager.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bt.TokenManager.Service
{
    public class TokenManagerService
    {
        private readonly IFetchGithubConfigurationService _fetchGithubConfigurationService;

        public TokenManagerService(IFetchGithubConfigurationService fetchGithubConfigurationService)
        {
            _fetchGithubConfigurationService = fetchGithubConfigurationService;
        }

        public async Task<string> ReadJwtToken(string authorization, string companyCode)
        {
            string userId = string.Empty;
            PublicKeyDetail publicKeyDetail = await GetTokenKey(companyCode);

            if (!string.IsNullOrEmpty(authorization))
            {
                string token = authorization.Replace("Bearer", "").Trim();
                if (!string.IsNullOrEmpty(token) && token != "null")
                {
                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = publicKeyDetail.Issuer,
                        ValidAudience = publicKeyDetail.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(publicKeyDetail.Key!))
                    }, out SecurityToken validatedToken);

                    var securityToken = handler.ReadToken(token) as JwtSecurityToken;
                    userId = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")!.Value;
                }
            }
            return userId;
        }

        private async Task<PublicKeyDetail> GetTokenKey(string companyCode)
        {
            var values = companyCode.Split("-");
            if (values.Length < 2)
            {
                throw new ArgumentException("Invalid company code used");
            }

            var publicKeys = _fetchGithubConfigurationService.GetPublicKeyConfiguration();
            // publicKeys.CompanyCode = values[1];

            // var jwtSetting = await Task.FromResult(_jwtTokenConfig.GetJwtSettingsDetail(values[0].ToLower()));
            return await Task.FromResult(publicKeys);
        }

        public async Task<string> GenerateAccessToken(RequestToken requestToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var num = new Random().Next(1, 10);

            await ValidateTokenGeneratorRequest(requestToken);
            PublicKeyDetail publicKeyDetail = await GetTokenKey(requestToken.CompanyCode!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(JwtRegisteredClaimNames.Sid, requestToken.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, requestToken.Email!),
                    new Claim(ClaimTypes.Role, requestToken.Role!),
                    new Claim(JwtRegisteredClaimNames.Aud, num.ToString()),
                    new Claim(ClaimTypes.Version, "1.0.0"),
                    new Claim(Constants.CompanyCode, publicKeyDetail.CompanyCode),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(Constants.JBot, JsonConvert.SerializeObject(requestToken))
                }),

                // ----------- Expiry time at after what time token will get expired -----------------------------
                Expires = DateTime.UtcNow.AddSeconds(publicKeyDetail.DefaulExpiryTimeInSeconds * 6),

                SigningCredentials = new SigningCredentials(
                                           new SymmetricSecurityKey(Encoding.UTF8.GetBytes(publicKeyDetail.Key)),
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
