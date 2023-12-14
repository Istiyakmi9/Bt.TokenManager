namespace Bot.App.ServiceManager.Model
{
    public class RequestToken
    {
        public string? UserDetail { set; get; }
        public long UserId { set; get; }
        public string? Email { set; get; }
        public string? CompanyCode { set; get; }
        public string? Role { set; get; }
    }
}
