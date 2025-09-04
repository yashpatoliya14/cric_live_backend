// CricLive/Models/Tournament.cs

namespace CricLive.Models
{
    public class Tournament
    {
        public int? TournamentId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; }
        public int HostId { get; set; } 
        public DateTime CreatedAt { get; set; }

        public List<Scorer>? Scorers { get; set; }
    }
}