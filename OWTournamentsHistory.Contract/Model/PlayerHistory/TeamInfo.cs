namespace OWTournamentsHistory.Contract.Model.PlayerHistory
{
    public class TeamInfo
    {
        public string CaptainName { get; set; }
        public int TournamentNumber { get; set; }
        public int Place { get; set; }
        public int MapsWon { get; set; }
        public int MapsPlayed { get; set; }
        public int MatchesPlayed { get; set; }
        public decimal? AverageMatchesCloseScore { get; set; }
        public ICollection<PlayerInTheTeamInfo> TeamMembers { get; set; }
        public ICollection<MatchInfo> TeamMatches { get; set; }

        public decimal Winrate => (decimal)MapsWon / MapsPlayed * 100; 
    }
}
