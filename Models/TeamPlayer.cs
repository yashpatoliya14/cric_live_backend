namespace CricLive.Models
{
    public class TeamPlayer
    {
        public int teamPlayerId { get; set; } 
        public int teamId { get; set; }

        public int uid { get; set; }
        public string? playerName { get; set; }
        public int? playerId { get; set; }
    }
}
