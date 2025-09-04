namespace CricLive.Models.Login
{
    public class LoginResponse
    {
        public int userId { get; set; }
        public string? email { get; set; }
        public string? token { get; set; }
    }
}
