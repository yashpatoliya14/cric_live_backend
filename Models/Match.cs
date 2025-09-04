namespace CricLive.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int InningNo { get; set; }
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public DateTime MatchDate { get; set; }
        public int Overs { get; set; }
        public string? Status { get; set; }
        public int? TossWon { get; set; }
        public string? Decision { get; set; }
        public int? TournamentId { get; set; }
        public int WideRun { get; set; }
        public int NoBallRun { get; set; }
        public int? StrikerBatsmanId { get; set; } // Nullable
        public int? NonStrikerBatsmanId { get; set; } // Nullable
        public int? BowlerId { get; set; } // Nullable
        public int? CurrentBattingTeamId { get; set; } // Nullable
        public int Uid { get; set; }
        public string MatchState { get; set; }
        public decimal CurrentOvers { get; set; }

        public string? Team1Name { get; set; }
        public string? Team2Name { get; set; }
    }
}
