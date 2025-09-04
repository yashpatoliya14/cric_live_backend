namespace CricLive.Models
{
    public class User
    {
        public int? Uid { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string Gender { get; set; }
        public string LastName { get; set; }

        public int? IsVerified { get; set; }

        public string? Role { get; set; }

        public string? ProfilePhoto { get; set; }


    }
}
