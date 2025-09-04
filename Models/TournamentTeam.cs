namespace CricLive.Models
{
    public class TournamentTeam
    {
        public int? TournamentTeamId { get; set; }
        public int TournamentId { get; set; }
        public int TeamId { get; set; }

        // Additional properties for returning useful data from JOINs
        public string? TeamName { get; set; }
        public string? Logo { get; set; }
    }
}
