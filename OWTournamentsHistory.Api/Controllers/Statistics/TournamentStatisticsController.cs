using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Contract.Model.Type;
using Player = OWTournamentsHistory.Contract.Model.PlayerStatistics;
using Tournament = OWTournamentsHistory.Contract.Model.TournamentStatistics;

namespace OWTournamentsHistory.Api.Controllers.Statistics
{
    public partial class StatisticsController : Controller
    {
        [HttpGet]
        [Route("/tournament/{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tournament.TournamentStatisticsInfo>> GetTournamentStatistics(int number, CancellationToken cancellationToken)
        {
            try
            {
                var tournamentTeams = await _teamRepository.GetAsync(team => team.TournamentNumber == number);
                var tournamentMatches = await _matchRepository.GetAsync(match => match.TournamentNumber == number);
                var tournamentTeamPlayers = tournamentTeams.SelectMany(team => team.Players.Select(p => new {Player  = p, p.IsNewPlayer, p.IsNewRole}).ToArray()).ToArray();

                if (!tournamentTeams.Any() && !tournamentMatches.Any()) 
                {
                    return NotFound();
                }

                var topTeams = tournamentTeams
                    .OrderBy(team => team.Place)
                    .Select(team => new Tournament.TournamentTeamInfo { Place = team.Place, CaptainName = team.CaptainName })
                    .Take(10)
                    .ToArray();

                var totalWinrate = tournamentTeams
                    .Select(team => team.MapsWon / (decimal)team.MapsPlayed)
                    .Average();

                var totalTeamWeight = tournamentTeams
                    .Average(team => team.Players.Average(player => player.Weight ?? 0));

                var totalAverageMatchesCloseScore = tournamentTeams
                    .Average(team => team.AverageMatchesCloseScore);

                var winnerTeam = tournamentTeams.Single(team => team.Place == 1);
                var winner = new Tournament.TeamStatistics
                {
                    WinRate = winnerTeam.MapsWon / (decimal)winnerTeam.MapsPlayed,
                    TotalWinRate = totalWinrate,
                    MatchesPlayed = winnerTeam.MatchesPlayed,
                    TeamWeight = winnerTeam.Players.Average(p => p.Weight ?? 0),
                    TotalTeamWeight = totalTeamWeight,
                    AverageMatchesCloseScore = winnerTeam.AverageMatchesCloseScore,
                    TotalAverageMatchesCloseScore = totalAverageMatchesCloseScore,
                    Players = winnerTeam.Players.Select(player => new Player.PlayerInTheTeamInfo
                    {
                        BattleTag = player.BattleTag,
                        DisplayWeight = player.DisplayWeight,
                        Division = player.Division,
                        Name = player.Name,
                        Role = (PlayerRole)player.Role,
                        SubRole = (PlayerSubRole?)player.SubRole
                    }).ToArray()
                };

                var allTeams = await _teamRepository.GetAsync();
                var allMatches = await _matchRepository.GetAsync();

                var tournamentsTotal = allTeams
                    .Select(team => team.TournamentNumber)
                    .Distinct()
                    .Count();

                var playersToDivisions = tournamentTeamPlayers
                    .GroupBy(player => player.Player.Division)
                    .Where(gr => gr.Key is not null)
                    .Select(gr => new Point2D<decimal> { X = gr.Key ?? 0, Y = gr.Count() })
                    .ToArray();

                var globalPlayersToDivisions = allTeams
                    .SelectMany(p => p.Players)
                    .GroupBy(p => p.Division)
                    .Where(gr => gr.Key is not null)
                    .Select(gr => new Point2D<decimal> { X = gr.Key ?? 0, Y = gr.Count() })
                    .ToArray();

                var matchesCloseness = tournamentMatches
                    .Select(match => match.Closeness == null ? (MatchCloseness?)null : (MatchCloseness)Math.Max(0, Math.Ceiling(match.Closeness.Value) - 1))
                    .GroupBy(m => m)
                    .Select(gr => new {gr.Key, Count = gr.Count()})
                    .ToArray();

                var globalMatchesCloseness = allMatches
                    .Select(match => match.Closeness == null ? (MatchCloseness?)null : (MatchCloseness)Math.Max(0, Math.Ceiling(match.Closeness.Value) - 1))
                    .GroupBy(m => m)
                    .Select(gr => new { gr.Key, Count = gr.Count() })
                    .ToArray();

                var matchesClosenessRelativeToAverage = matchesCloseness
                    .Select(closeness => new Point2DWithLabel<decimal>
                    { 
                        Label = closeness.Key?.ToString(), 
                        X = closeness?.Count ?? 0, 
                        Y = globalMatchesCloseness.Single(c => c.Key == closeness?.Key)?.Count / (decimal)tournamentsTotal
                    })
                    .ToArray();

                return new Tournament.TournamentStatisticsInfo
                {
                    TournamentNumber = number,
                    TeamsCount = tournamentTeams.Count,
                    MatchesCount = tournamentMatches.Count,
                    MapsPlayed = tournamentMatches.Sum(match => match.ScoreTeam1 + match.ScoreTeam2),
                    PlayersCount = tournamentTeamPlayers.Length,
                    NewPlayers = tournamentTeamPlayers.Count(player => player.IsNewPlayer),
                    NewRolePlayers = tournamentTeamPlayers.Count(player => player.IsNewRole) - tournamentTeamPlayers.Count(player => player.IsNewPlayer),
                    TopTeams = topTeams,
                    WinnerTeam = winner,
                    PlayersToDivisions = playersToDivisions,
                    GlobalPlayersToDivisions = globalPlayersToDivisions,
                    MatchesClosenessRelativeToAverage = matchesClosenessRelativeToAverage
                };
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }
    }
}
