namespace OWTournamentsHistory.Contract.Model
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Team
    {
        public long Id { get; set; }
        public int TournamentNumber { get; set; }
        public ICollection<TeamPlayerInfo> Players { get; set; }

        public int Place { get; set; }
        public string CaptainName { get; set; }
        public int MatchesPlayed { get; set; }
        public int MapsPlayed { get; set; }
        public int MapsWon { get; set; }
        public decimal? AverageMatchesCloseScore { get; set; }

        public decimal WinRate => MatchesPlayed == 0 ? 0 : MapsWon / MapsPlayed;
        public bool IsChampion => Place == 1;
        public decimal TeamWeight => Players.Sum(p => p.Weight ?? 0) / Players.Count;
    }
}
