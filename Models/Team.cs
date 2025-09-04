namespace CricLive.Models
{
    public class Team
    {
        public int? teamId { get; set; }
        public string teamName { get; set; }

        public string? logo { get; set; }

        public int? tournamentId { get; set; }

        public int? Uid { get; set; }
    }
}
