namespace CricLive.Models
{
    public class UpdateMatchDto
    {
        // --- Match Status & Progress ---
        public string? Status { get; set; }
        public int? InningNo { get; set; }
        public string? MatchState { get; set; }
        public decimal? CurrentOvers { get; set; }
        public DateTime? MatchDate { get; set; }

        // --- Toss Details (Newly Added) ---
        public int? TossWon { get; set; }
        public string? Decision { get; set; }

        // --- Current Player IDs ---
        public int? StrikerBatsmanId { get; set; }
        public int? NonStrikerBatsmanId { get; set; }
        public int? BowlerId { get; set; }
        public int? CurrentBattingTeamId { get; set; }

        // --- Run Details ---
        public int? WideRun { get; set; }
        public int? NoBallRun { get; set; }
    }
}