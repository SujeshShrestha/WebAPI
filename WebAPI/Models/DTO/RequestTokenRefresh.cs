namespace WebAPI.Models.DTO
{
    public class RequestTokenRefresh
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
