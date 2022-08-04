namespace HelpDeskAPI.Models
{
    public class TokenInter
    {
        public string access_token { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
    }
}
