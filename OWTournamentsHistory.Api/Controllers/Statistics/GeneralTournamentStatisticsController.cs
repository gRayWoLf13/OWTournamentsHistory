using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.Contract.Model.GeneralTournamentStatistics;
using OWTournamentsHistory.Contract.Model.Type;
using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.Controllers.Statistics
{
    public partial class StatisticsController : Controller
    {
        [HttpGet]
        [Route("/tournaments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralTournamentStatisticsInfo>> GetGeneralTournamentStatistics(CancellationToken cancellationToken)
        {
            try
            {
                var allTeams = await _teamRepository.GetAsync(cancellationToken);
                var allMatches = await _matchRepository.GetAsync(cancellationToken);
                var allPlayers = await _playerRepository.GetAsync(cancellationToken);

                var tournamentsCount = allTeams.Select(team => team.TournamentNumber).Distinct().Count();
                var championsCount = allTeams.Where(team => team.Place == 1).Select(team => team.Players.Count).Sum();

                var matchesWithPlayers = allMatches.Select(match => new
                {
                    Match = match,
                    PlayersTeam1 = allTeams.Single(team => team.TournamentNumber == match.TournamentNumber && team.CaptainName == match.Team1CaptainName).Players,
                    PlayersTeam2 = allTeams.Single(team => team.TournamentNumber == match.TournamentNumber && team.CaptainName == match.Team2CaptainName).Players,
                });


                var averageMatchClosenessToTournament = allMatches
                    .GroupBy(match => match.TournamentNumber)
                    .Select(gr => (TournamentNumber: gr.Key, Closeness: gr.Average(match => match.Closeness ?? 0)));

                var tournamentStats = allTeams
                    .GroupBy(team => team.TournamentNumber)
                    .Select(gr => (TournamentNumber: gr.Key, PlayersCount: gr.Sum(team => team.Players.Count), TeamWeight: gr.Average(team => team.Players.Average(player => player.Weight ?? 0))))
                    .ToArray();


                var averageDivisionToTournament = allTeams
                 .GroupBy(team => team.TournamentNumber)
                 .Select(gr => (TournamentNumber: gr.Key,
                     Tank: (decimal?)gr.SelectMany(team => team.Players.Where(player => player.Role == DataAccess.Model.Type.TeamPlayerRole.Tank)).Average(player => player.Division),
                     Dps: (decimal?)gr.SelectMany(team => team.Players.Where(player => player.Role == DataAccess.Model.Type.TeamPlayerRole.Dps)).Average(player => player.Division),
                     Support: (decimal?)gr.SelectMany(team => team.Players.Where(player => player.Role == DataAccess.Model.Type.TeamPlayerRole.Support)).Average(player => player.Division)))
                 .ToArray();

                var topPlayersScore = (from match in allMatches
                                       let playersTeam1 = allTeams.SingleOrDefault(team => team.TournamentNumber == match.TournamentNumber && NameExtensions.EqualsIgnoreCase(team.CaptainName, match.Team1CaptainName))?.Players
                                       let playersTeam2 = allTeams.SingleOrDefault(team => team.TournamentNumber == match.TournamentNumber && NameExtensions.EqualsIgnoreCase(team.CaptainName, match.Team2CaptainName))?.Players
                                       let scorePlayers = (playersTeam1 ?? Array.Empty<TeamPlayerInfo>()).Select(player => (PlayerName: player.Name, Score: match.ScoreTeam1))
                                           .Concat((playersTeam2 ?? Array.Empty<TeamPlayerInfo>()).Select(player => (PlayerName: player.Name, Score: match.ScoreTeam2)))
                                       select scorePlayers)
                                        .SelectMany(p => p)
                                        .GroupBy(item => item.PlayerName)
                                        .Select(item => (PlayerName: item.Key, Score: item.Sum(p => p.Score)))
                                        .OrderByDescending(item => item.Score)
                                        .Take(20);

                var topChampions = allTeams
                    .Where(team => team.Place == 1)
                    .SelectMany(team => team.Players.Select(player => (PlayerName: player.Name, Role: player.Role)))
                    .GroupBy(player => player.PlayerName)
                    .Select(item => (PlayerName: item.Key, Roles: item.Select(i => i.Role).ToArray()))
                    .OrderByDescending(item => item.Roles.Length)
                    .Take(20);


                var topWinRate = allTeams
                    .SelectMany(team => team.Players.Select(player => (PlayerName: player.Name, WinRate: team.MatchesPlayed == 0 ? 0 : team.MapsWon / (decimal)team.MapsPlayed)))
                    .GroupBy(player => player.PlayerName)
                    .Select(item => (PlayerName: item.Key, WinRate: item.Average(i => i.WinRate)))
                    .OrderByDescending(item => item.WinRate)
                    .Take(20);

                var top0Wins = allTeams
                    .Where(team => team.MapsWon == 0)
                    .SelectMany(team => team.Players.Select(player => (PlayerName: player.Name, Role: player.Role)))
                    .GroupBy(player => player.PlayerName)
                    .Select(item => (PlayerName: item.Key, Roles: item.Select(i => i.Role).ToArray()))
                    .OrderByDescending(item => item.Roles.Length)
                    .Take(20);

                var bestDuos =
                    await _playerDuosRepository.GetSortedAsync(orderingKey: duo => duo.MapsWon, sortAscending: false, limit: 20);

                var worstDuos = 
                    await _playerDuosRepository.GetByWinRate(sortAscending: true, limit: 20);


                var playerRolesToDivisions = allTeams
                    .SelectMany(team => team.Players.Where(player => player.Division is not null).Select(player => (Division: (int)player.Division, player.Role)))                    
                    .GroupBy(player => player.Division)
                    .Select(gr => (Division: gr.Key,
                        TankCount: gr.Count(player => player.Role == DataAccess.Model.Type.TeamPlayerRole.Tank),
                        DpsCount: gr.Count(player => player.Role == DataAccess.Model.Type.TeamPlayerRole.Dps),
                        SupportCount: gr.Count(player => player.Role == DataAccess.Model.Type.TeamPlayerRole.Support)))
                    .ToArray();

                return new GeneralTournamentStatisticsInfo
                {
                    TournamentsCount = tournamentsCount,
                    TeamsCount = allTeams.Count,
                    PlayersCount = allPlayers.Count,
                    MatchesCount = allMatches.Count,
                    OWALsCount = 1,
                    ChampionsCount = championsCount,

                    TopMapsWon = topPlayersScore.Select(player => (Point2D<string>)player).ToArray(),
                    TopChampions = topChampions.Select(player => new Point2DWithLabel<string> { X = player.PlayerName, Label = string.Join(", ", player.Roles.Distinct()), Y = player.Roles.Length }).ToArray(),
                    TopWinRate = topWinRate.Select(player => (Point2D<string>)player).ToArray(),
                    Top0Wins = top0Wins.Select(player => new Point2DWithLabel<string> { X = player.PlayerName, Label = string.Join(", ", player.Roles.Distinct()), Y = player.Roles.Length }).ToArray(),
                    BestDuos = bestDuos.Select(duo => new Point2D<string> { X = $"{duo.Player1} - {duo.Player2}", Y = duo.MapsWon }).ToArray(),
                    WorstDuos = worstDuos.Select(duo => new Point2D<string> { X = $"{duo.Player1} - {duo.Player2}", Y = duo.MapsWon / (decimal)duo.MapsPlayed }).ToArray(),

                    AverageTankDivisionToTournament = averageDivisionToTournament.Select(item => new Point2D<decimal> { X = item.TournamentNumber, Y = item.Tank }).ToArray(),
                    AverageDpsDivisionToTournament = averageDivisionToTournament.Select(item => new Point2D<decimal> { X = item.TournamentNumber, Y = item.Dps }).ToArray(),
                    AverageSupportDivisionToTournament = averageDivisionToTournament.Select(item => new Point2D<decimal> { X = item.TournamentNumber, Y = item.Support }).ToArray(),

                    AverageMatchClosenessToTournament = averageMatchClosenessToTournament.Select(item => (Point2D<decimal>)item).ToArray(),
                    AverageTeamWeightToTournament = tournamentStats.Select(item => new Point2D<decimal> { X = item.TournamentNumber, Y = item.TeamWeight }).ToArray(),
                    PlayersCountToTournament = tournamentStats.Select(item => new Point2D<decimal> { X = item.TournamentNumber, Y = item.PlayersCount }).ToArray(),

                    TankPlayersToDivision = playerRolesToDivisions.Select(item => new Point2D<decimal> { X = item.Division, Y = item.TankCount }).ToArray(),
                    DpsPlayersToDivision = playerRolesToDivisions.Select(item => new Point2D<decimal> { X = item.Division, Y = item.DpsCount }).ToArray(),
                    SupportPlayersToDivision = playerRolesToDivisions.Select(item => new Point2D<decimal> { X = item.Division, Y = item.SupportCount }).ToArray(),
                };
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }
    }
}
