namespace OWTournamentsHistory.Contract.Model
{
    public class Team
    {
        public long Id { get; set; }
        public int TournamentNumber { get; set; }
        public required ICollection<TeamPlayerInfo> Players { get; set; }

        public int Place { get; set; }
        public required string CaptainName { get; set; }
        public int MatchesPlayed { get; set; }
        public int MapsPlayed { get; set; }
        public int MapsWon { get; set; }
        public decimal? AverageMatchesCloseScore { get; set; }

        public decimal WinRate => MatchesPlayed == 0 ? 0 : MapsWon / (decimal)MapsPlayed;
        public bool IsChampion => Place == 1;
        public decimal TeamWeight => Players.Sum(p => p.Weight ?? 0) / Players.Count;
    }
}
