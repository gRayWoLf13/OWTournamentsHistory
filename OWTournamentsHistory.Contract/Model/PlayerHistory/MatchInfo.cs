namespace OWTournamentsHistory.Contract.Model.PlayerHistory
{
    public class MatchInfo
    {
        public string CaptainTeam1 { get; set; }
        public string CaptainTeam2 { get; set; }
        public int ScoreTeam1 { get; set; }
        public int ScoreTeam2 { get; set; }
        public decimal? Closeness { get; set; }

        public string Score => $"{ScoreTeam1}-{ScoreTeam2}";
        public string MatchName => $"{CaptainTeam1}-{CaptainTeam2}";
    }
}
