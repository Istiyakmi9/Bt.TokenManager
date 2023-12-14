namespace Bot.App.ServiceManager.Model
{
    public class JwtSetting
    {
        public string? Key { set; get; }
        public string? Issuer { get; set; }
        public long DefaulExpiryTimeInSeconds { set; get; }
        public long DefaultRefreshTokenExpiryTimeInSeconds { set; get; }
    }
}
