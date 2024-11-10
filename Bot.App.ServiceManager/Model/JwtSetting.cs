namespace Bot.App.ServiceManager.Model
{
    public class JwtTokenConfig
    {
        public Dictionary<string, JwtSetting>? JwtSettings { set; get; }

        public JwtSetting GetJwtSettingsDetail(string code)
        {
            JwtSetting jwtSetting = new JwtSetting();
            switch (code)
            {
                case "ems":
                    JwtSettings!.TryGetValue("emstum", out jwtSetting!);
                    break;
                case "hb":
                    JwtSettings!.TryGetValue("hiringbell", out jwtSetting!);
                    break;
                case "btcm":
                    JwtSettings!.TryGetValue("generic", out jwtSetting!);
                    break;
                default:
                    // JwtSettings!.TryGetValue("generic", out jwtSetting!);
                    throw new UnauthorizedAccessException("Token code not found. Please provide code to generate token.");
            }

            return jwtSetting;
        }
    }

    public class JwtSetting
    {
        public string? Key { set; get; }
        public string? Issuer { get; set; }
        public string? CompanyCode { get; set; }
        public long DefaulExpiryTimeInSeconds { set; get; }
        public long DefaultRefreshTokenExpiryTimeInSeconds { set; get; }
    }
}
