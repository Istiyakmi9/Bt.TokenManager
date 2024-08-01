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
                case "bh":
                    JwtSettings!.TryGetValue("hiringbell", out jwtSetting!);
                    break;
                default:
                    JwtSettings!.TryGetValue("generic", out jwtSetting!);
                    break;
            }

            return jwtSetting;
        }
    }

    public class JwtSetting
    {
        public string? Key { set; get; }
        public string? Issuer { get; set; }
        public long DefaulExpiryTimeInSeconds { set; get; }
        public long DefaultRefreshTokenExpiryTimeInSeconds { set; get; }
    }
}
