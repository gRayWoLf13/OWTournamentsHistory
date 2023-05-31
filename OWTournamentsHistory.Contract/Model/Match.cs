using OWTournamentsHistory.Contract.Model.Type;

namespace OWTournamentsHistory.Contract.Model
{
    public class Match
    {
        public long Id { get; set; }
        public int TournamentNumber { get; set; }
        public int MatchNumber { get; set; }
        public string Team1CaptainName { get; set; }
        public string Team2CaptainName { get; set; }
        public int ScoreTeam1 { get; set; }
        public int ScoreTeam2{ get; set; }
        public decimal? Closeness { get; set; }

        public string Score => $"{ScoreTeam1}-{ScoreTeam2}";
        public string MatchName => $"{Team1CaptainName}-{Team2CaptainName}";
        public decimal? ClosenessPercent => Closeness == null ? null : (5 - Closeness) / 5m * 100m; 
        public MatchCloseness? MatchCloseness => Closeness == null ? null : (MatchCloseness)(Math.Ceiling(Closeness.Value) - 1);
    }
}
