namespace CricLive.Models
{
    public class CreateMatchDto
    {
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public DateTime MatchDate { get; set; }
        public int Overs { get; set; }
        public int? TossWon { get; set; }
        public string? Decision { get; set; }
        public int? TournamentId { get; set; }

        public int? StrikerBatsmanId { get; set; }
        public int? NonStrikerBatsmanId { get; set; }
        public int? CurrentBattingTeamId { get; set; }
        public int? BowlerId { get; set; }
        public int NoBallRun { get; set; }
        public int WideRun{ get; set; }

        public int Uid { get; set; }
        public string? MatchState { get; set; }
    }
}
