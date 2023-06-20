using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.Contract.Model.GeneralTournamentStatistics;
using OWTournamentsHistory.Contract.Model.PlayerStatistics;
using OWTournamentsHistory.Contract.Model.Type;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Model;
using OWTournamentsHistory.DataAccess.Model.Type;
using System.Diagnostics;
using Tournament = OWTournamentsHistory.Contract.Model.TournamentStatistics;

namespace OWTournamentsHistory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
#if DEBUG
    [AllowAnonymous]
#endif
    public class StatisticsController : Controller
    {
        private readonly TeamPlayerRole[] _possiblePlayerRoles = new[] { TeamPlayerRole.Tank, TeamPlayerRole.Dps, TeamPlayerRole.Support };

        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerOpponentsRepository _playerOpponentsRepository;
        private readonly IPlayerDuosRepository _playerDuosRepository;
        private readonly IGeneralTournamentStatsRepository _generalTournamentStatsRepository;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            IMatchRepository matchRepository,
            IPlayerRepository playerRepository,
            ITeamRepository teamRepository,
            IPlayerOpponentsRepository playerOpponentsRepository,
            IPlayerDuosRepository playerDuosRepository,
            IGeneralTournamentStatsRepository generalTournamentStatsRepository,
            ILogger<StatisticsController> logger)
        {
            _matchRepository = matchRepository;
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
            _playerOpponentsRepository = playerOpponentsRepository;
            _playerDuosRepository = playerDuosRepository;
            _generalTournamentStatsRepository = generalTournamentStatsRepository;
            _logger = logger;
        }

        [HttpGet]
        [Route("Player/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PlayerStatisticsInfo>> GetPlayerStatistics(string name, CancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var player = await _playerRepository.FindPlayer(name);
                if (player == null)
                {
                    return NotFound();
                }
                var playerName = player.Name;
                var playerTeams = await _teamRepository.FindTeamsByPlayerName(playerName);
                var playerCaptainsByTournamentNumber = playerTeams.ToDictionary(team => team.TournamentNumber, team => team.CaptainName);
                var playerMatches = await _matchRepository.Find(playerTeams.Select(team => (team.TournamentNumber, team.CaptainName)).ToArray());

                var tournamentsPlayed = playerTeams.Count;
                var tournamentsWon = playerTeams.Where(team => team.Place == 1).Count();
                var tournamentsWith0Wins = playerMatches
                    .GroupBy(match => match.TournamentNumber)
                    .Where(matches => matches.Sum(match => GetNormalizedMatchScore(match, playerCaptainsByTournamentNumber[matches.Key]).Score) == 0)
                    .Count();
                var averagePlayerWinrate = playerMatches
                    .Sum(match => GetNormalizedMatchScore(match, playerCaptainsByTournamentNumber[match.TournamentNumber]).Score)
                    / (decimal)playerMatches.Sum(match => match.ScoreTeam1 + match.ScoreTeam2);

                var bestResult = playerTeams.OrderBy(team => team.Place).First();
                var bestResultPlace = bestResult.Place;
                var bestResultTournamentNumber = bestResult.TournamentNumber;
                var bestResultCaptain = bestResult.CaptainName;

                var playerTeamsWithCloseness = playerTeams.Where(team => team.AverageMatchesCloseScore is not null).ToArray();
#pragma warning disable CS8629 // Nullable value type may be null.
                var averagePlayerCloseness = playerTeamsWithCloseness.Any()
                    ? playerTeamsWithCloseness.Average(team => team.AverageMatchesCloseScore.Value)
                    : 0;
#pragma warning restore CS8629 // Nullable value type may be null.
                var mapsWon = playerMatches.Sum(match => GetNormalizedMatchScore(match, playerCaptainsByTournamentNumber[match.TournamentNumber]).Score);
                var mapsPlayed = playerTeams.Sum(team => team.MapsPlayed);
                var averagePlace = (int)Math.Round(playerTeams.Average(team => team.Place), 0);

                var playerRoleInfos = new Dictionary<PlayerRole, PlayerRoleInfo>();
                foreach (var role in _possiblePlayerRoles)
                {
                    var teamsOnRole = playerTeams
                        .Where(team => team.Players.Single(player => NameExtensions.EqualsIgnoreCase(player.Name, playerName)).Role == role)
                        .ToArray();

                    var tournamentsOnRole = teamsOnRole.Select(team => team.TournamentNumber).ToHashSet();

                    var roleTournamentsPlayed = teamsOnRole.Length;

                    if (roleTournamentsPlayed == 0)
                    {
                        playerRoleInfos.Add((PlayerRole)role, new PlayerRoleInfo
                        {
                            TournamentsPlayed = 0,
                            AverageWinrate = 0,
                            LastDisplayWeight = null,
                            LastDivision = null,
                        });
                        continue;
                    }

                    //captains of player teams where he played on selected role
                    var captainsFromRole = teamsOnRole
                        .Select(team => team.CaptainName)
                        .ToHashSet();

                    var playerRoleMatches = playerMatches
                        .Where(match => tournamentsOnRole.Contains(match.TournamentNumber)
                            && (captainsFromRole.Contains(match.Team1CaptainName) || captainsFromRole.Contains(match.Team2CaptainName)))
                        .ToArray();

                    var roleAverageWinrate = playerRoleMatches
                        .GroupBy(match => match.TournamentNumber)
                        .Average(matches => matches.Sum(m => GetNormalizedMatchScore(m, playerCaptainsByTournamentNumber[matches.Key]).Score) / (decimal)matches.Sum(m => m.ScoreTeam1 + m.ScoreTeam2));

                    var roleLastTournamentNumber = teamsOnRole
                        .Max(team => team.TournamentNumber);

                    var roleLastPlayer = playerTeams
                        .Single(team => team.TournamentNumber == roleLastTournamentNumber && team.Players.Any(player => NameExtensions.EqualsIgnoreCase(player.Name, playerName) && player.Role == role))
                        .Players
                        .Single(player => NameExtensions.EqualsIgnoreCase(player.Name, playerName) && player.Role == role);
                    var roleLastPlayerDivision = roleLastPlayer.Division;
                    var roleLastPlayerDisplayWeight = roleLastPlayer.DisplayWeight;

                    playerRoleInfos.Add((PlayerRole)role, new PlayerRoleInfo
                    {
                        TournamentsPlayed = roleTournamentsPlayed,
                        AverageWinrate = roleAverageWinrate,
                        LastDivision = roleLastPlayerDivision,
                        LastDisplayWeight = roleLastPlayerDisplayWeight,
                    });
                }

                var teamInfos = new List<PlayerTeamInfo>();
                foreach (var team in playerTeams)
                {
                    var matchesPlayedByTeam = playerMatches
                        .Where(match => match.TournamentNumber == team.TournamentNumber
                            && (NameExtensions.EqualsIgnoreCase(match.Team1CaptainName, team.CaptainName) || NameExtensions.EqualsIgnoreCase(match.Team2CaptainName, team.CaptainName)))
                        .ToArray();

                    var teamMatches = matchesPlayedByTeam.Select(match => new MatchInfo
                    {
                        CaptainTeam1 = NameExtensions.EqualsIgnoreCase(team.CaptainName, match.Team1CaptainName) ? match.Team1CaptainName : match.Team2CaptainName,
                        CaptainTeam2 = !NameExtensions.EqualsIgnoreCase(team.CaptainName, match.Team1CaptainName) ? match.Team1CaptainName : match.Team2CaptainName,
                        ScoreTeam1 = NameExtensions.EqualsIgnoreCase(team.CaptainName, match.Team1CaptainName) ? match.ScoreTeam1 : match.ScoreTeam2,
                        ScoreTeam2 = !NameExtensions.EqualsIgnoreCase(team.CaptainName, match.Team1CaptainName) ? match.ScoreTeam1 : match.ScoreTeam2,
                        Closeness = match.Closeness,
                    }).ToArray();


                    var teamMembers = new List<PlayerInTheTeamInfo>();
                    foreach (var teamPlayer in team.Players)
                    {
                        teamMembers.Add(new PlayerInTheTeamInfo
                        {
                            Role = (PlayerRole)teamPlayer.Role,
                            SubRole = (PlayerSubRole?)teamPlayer.SubRole,
                            Name = teamPlayer.Name,
                            BattleTag = teamPlayer.BattleTag,
                            Division = teamPlayer.Division,
                            DisplayWeight = teamPlayer.DisplayWeight,
                        });
                    }

                    teamInfos.Add(new PlayerTeamInfo
                    {
                        CaptainName = team.CaptainName,
                        TournamentNumber = team.TournamentNumber,
                        Place = team.Place,
                        MapsWon = matchesPlayedByTeam
                            .Sum(match => GetNormalizedMatchScore(match, playerCaptainsByTournamentNumber[match.TournamentNumber]).Score),
                        MapsPlayed = matchesPlayedByTeam.Sum(match => match.ScoreTeam1 + match.ScoreTeam2),
                        AverageMatchesCloseScore = team.AverageMatchesCloseScore,
                        MatchesPlayed = team.MatchesPlayed,
                        TeamMatches = teamMatches,
                        TeamMembers = teamMembers,
                    });
                }

                //charts data calculation
                var tankPoints = new Point2D<decimal?>[playerTeams.Count];
                var dpsPoints = new Point2D<decimal?>[playerTeams.Count];
                var supportPoints = new Point2D<decimal?>[playerTeams.Count];
                var placePoints = new Point2D<decimal?>[playerTeams.Count];
                var counter = 0;
                foreach (var team in playerTeams)
                {
                    var teamPlayerInfo = team.Players.Single(player => NameExtensions.EqualsIgnoreCase(player.Name, name));
                    tankPoints[counter] = new Point2D<decimal?> { X = team.TournamentNumber, Y = teamPlayerInfo.Role == TeamPlayerRole.Tank ? teamPlayerInfo.Weight : null };
                    dpsPoints[counter] = new Point2D<decimal?> { X = team.TournamentNumber, Y = teamPlayerInfo.Role == TeamPlayerRole.Dps ? teamPlayerInfo.Weight : null };
                    supportPoints[counter] = new Point2D<decimal?> { X = team.TournamentNumber, Y = teamPlayerInfo.Role == TeamPlayerRole.Support ? teamPlayerInfo.Weight : null };
                    placePoints[counter++] = new Point2D<decimal?> { X = team.TournamentNumber, Y = team.Place };
                }

                var standardDeviation = new List<Point2DWithLabel<decimal>>();
                var generalTournamentStats = await _generalTournamentStatsRepository.GetStats();

                standardDeviation.Add(new Point2DWithLabel<decimal> { Label = "Winrate", X = averagePlayerWinrate, Y = generalTournamentStats.AverageWinRate });
                standardDeviation.Add(new Point2DWithLabel<decimal> { Label = "Closeness", X = averagePlayerCloseness, Y = generalTournamentStats.StatsToTournaments.Average(item => item.Value.AverageCloseness) });
                standardDeviation.Add(new Point2DWithLabel<decimal> { Label = "Tournaments played", X = tournamentsPlayed / (decimal)generalTournamentStats.TournamentsCount, Y = generalTournamentStats.AverageTournamentsPlayedPercentage });

                foreach (var role in _possiblePlayerRoles)
                {
                    var teamsOnRole = playerTeams
                        .Where(team => team.Players.Single(player => NameExtensions.EqualsIgnoreCase(player.Name, playerName)).Role == role)
                        .ToArray();

                    var roleWinrate = teamsOnRole.Any()
                        ? teamsOnRole.Average(player => player.MapsWon / (decimal)player.MapsPlayed)
                        : 0;

                    standardDeviation.Add(new Point2DWithLabel<decimal> { Label = role.ToString(), X = roleWinrate, Y = generalTournamentStats.RoleWinRates.Single(info => info.Role == role).AverageWinRate });
                }


                //combinations data calculation
                var wonAgainst1 = await _playerOpponentsRepository.GetSortedAsync(pp => pp.MapsWon, sortAscending: false, predicate: pp => pp.Player1 == playerName, limit: 10);

                var wonAgainst2 = (await _playerOpponentsRepository.GetTopLossesAgainst(playerName, limit: 10))
                    .Select(item => new PlayerPair { Player1 = item.Player2, Player2 = item.Player1, MapsPlayed = item.MapsPlayed, MapsWon = item.MapsPlayed - item.MapsWon })
                    .ToArray();

                var wonAgainst = wonAgainst1.Union(wonAgainst2).OrderByDescending(win => win.MapsWon).Take(10).ToArray();

                var lostAgainst1 = await _playerOpponentsRepository.GetSortedAsync(pp => pp.MapsWon, sortAscending: false, predicate: pp => pp.Player2 == playerName, limit: 10);

                var lostAgainst2 = (await _playerOpponentsRepository.GetTopWinsAgainst(playerName, limit: 10))
                    .Select(item => new PlayerPair { Player1 = item.Player2, Player2 = item.Player1, MapsPlayed = item.MapsPlayed, MapsWon = item.MapsPlayed - item.MapsWon })
                    .ToArray();

                var lostAgainst = lostAgainst1.Union(lostAgainst2).OrderByDescending(win => win.MapsWon).Take(10).ToArray();

                var info = new PlayerStatisticsInfo
                {
                    Name = player.Name,
                    BattleTags = player.BattleTags,
                    TwitchId = player.TwitchId,
                    TournamentsPlayed = tournamentsPlayed,
                    TournamentsWon = tournamentsWon,
                    TournamentsWith0Wins = tournamentsWith0Wins,
                    GlobalWinRate = averagePlayerWinrate,
                    BestResultTournamentNumber = bestResultTournamentNumber,
                    BestResultCaptainName = bestResultCaptain,
                    BestResultPlace = bestResultPlace,
                    AverageCloseness = averagePlayerCloseness,
                    MapsWon = mapsWon,
                    MapsPlayed = mapsPlayed,
                    AveragePlace = averagePlace,
                    RoleInfos = playerRoleInfos,
                    Teams = teamInfos,
                    TankPriceData = tankPoints,
                    DpsPriceData = dpsPoints,
                    SupportPriceData = supportPoints,
                    PlaceData = placePoints,
                    StandardDeviation = standardDeviation,
                    MostWinsAgainst = wonAgainst.Select(pp => new NameCount { Name = pp.Player2, Count = pp.MapsWon }).ToArray(),
                    MostLossesAgainst = lostAgainst.Select(pp => new NameCount { Name = pp.Player1, Count = pp.MapsWon }).ToArray(),
                };

                stopwatch.Stop();
                Debug.WriteLine(stopwatch.Elapsed);
                return info;

            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpGet]
        [Route("Tournaments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralTournamentStatisticsInfo>> GetGeneralTournamentStatistics(CancellationToken cancellationToken)
        {
            try
            {
                var generalTournamentStats = await _generalTournamentStatsRepository.GetStats();

                var bestDuos =
                    await _playerDuosRepository.GetSortedAsync(orderingKey: duo => duo.MapsWon, sortAscending: false, limit: 20);

                var worstDuos =
                    await _playerDuosRepository.GetByWinRate(sortAscending: true, limit: 20);

                return new GeneralTournamentStatisticsInfo
                {
                    TournamentsCount = generalTournamentStats.TournamentsCount,
                    TeamsCount = generalTournamentStats.StatsToTournaments.Sum(tournament => tournament.Value.TeamsCount),
                    PlayersCount = generalTournamentStats.StatsToTournaments.Sum(tournament => tournament.Value.PlayersCount),
                    MatchesCount = generalTournamentStats.StatsToTournaments.Sum(tournament => tournament.Value.MatchesCount),
                    OWALsCount = generalTournamentStats.OWALsCount,
                    ChampionsCount = generalTournamentStats.ChampionsCount,

                    TopMapsWon = generalTournamentStats.TopPlayersByScore.Select(item => new Point2D<string> { X = item.Key, Y = item.Value }).ToArray(),
                    TopChampions = generalTournamentStats.TopChampions.Select(item => new Point2DWithLabel<string> { X = item.Key, Label = string.Join(", ", item.Value.Roles.Distinct()), Y = item.Value.Roles.Length }).ToArray(),
                    TopWinRate = generalTournamentStats.TopWinRate.Select(item => new Point2D<string> { X = item.Key, Y = item.Value }).ToArray(),
                    Top0Wins = generalTournamentStats.Top0Wins.Select(item => new Point2DWithLabel<string> { X = item.Key, Label = string.Join(", ", item.Value.Roles.Distinct()), Y = item.Value.Roles.Length }).ToArray(),
                    BestDuos = bestDuos.Select(duo => new Point2D<string> { X = $"{duo.Player1} - {duo.Player2}", Y = duo.MapsWon }).ToArray(),
                    WorstDuos = worstDuos.Select(duo => new Point2D<string> { X = $"{duo.Player1} - {duo.Player2}", Y = duo.MapsWon / (decimal)duo.MapsPlayed }).ToArray(),

                    AverageTankDivisionToTournament = generalTournamentStats.StatsToTournaments.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.AverageDivisionToRole.Single(role => role.Role == TeamPlayerRole.Tank).Division }).ToArray(),
                    AverageDpsDivisionToTournament = generalTournamentStats.StatsToTournaments.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.AverageDivisionToRole.Single(role => role.Role == TeamPlayerRole.Dps).Division }).ToArray(),
                    AverageSupportDivisionToTournament = generalTournamentStats.StatsToTournaments.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.AverageDivisionToRole.Single(role => role.Role == TeamPlayerRole.Support).Division }).ToArray(),

                    AverageMatchClosenessToTournament = generalTournamentStats.StatsToTournaments.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.AverageCloseness }).ToArray(),
                    AverageTeamWeightToTournament = generalTournamentStats.StatsToTournaments.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.AverageTeamWeight }).ToArray(),
                    PlayersCountToTournament = generalTournamentStats.StatsToTournaments.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.PlayersCount }).ToArray(),

                    TankPlayersToDivision = generalTournamentStats.PlayerRolesToDivisions.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.TanksCount }).ToArray(),
                    DpsPlayersToDivision = generalTournamentStats.PlayerRolesToDivisions.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.DpsCount }).ToArray(),
                    SupportPlayersToDivision = generalTournamentStats.PlayerRolesToDivisions.Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value.SupportsCount }).ToArray(),
                };
            }
            catch (Exception ex)
            {
                return WrapException(ex);
            }
        }

        [HttpGet]
        [Route("Tournament/{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tournament.TournamentStatisticsInfo>> GetTournamentStatistics(int number, CancellationToken cancellationToken)
        {
            try
            {
                var tournamentTeams = await _teamRepository.GetAsync(team => team.TournamentNumber == number);
                var tournamentMatches = await _matchRepository.GetAsync(match => match.TournamentNumber == number);
                var tournamentTeamPlayers = tournamentTeams.SelectMany(team => team.Players.Select(p => new { Player = p, p.IsNewPlayer, p.IsNewRole }).ToArray()).ToArray();

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
                    Players = winnerTeam.Players.Select(player => new PlayerInTheTeamInfo
                    {
                        BattleTag = player.BattleTag,
                        DisplayWeight = player.DisplayWeight,
                        Division = player.Division,
                        Name = player.Name,
                        Role = (PlayerRole)player.Role,
                        SubRole = (PlayerSubRole?)player.SubRole
                    }).ToArray()
                };

                var generalTournamentStats = await _generalTournamentStatsRepository.GetStats();

                var tournamentsTotal = generalTournamentStats.TournamentsCount;

                var playersToDivisions = tournamentTeamPlayers
                    .GroupBy(player => player.Player.Division)
                    .Where(gr => gr.Key is not null)
                    .Select(gr => new Point2D<decimal> { X = gr.Key ?? 0, Y = gr.Count() })
                    .ToArray();

                var globalPlayersToDivisions = generalTournamentStats.PlayersToDivisionsCount
                    .Select(item => new Point2D<decimal> { X = int.Parse(item.Key), Y = item.Value })
                    .ToArray();

                var matchesCloseness = tournamentMatches
                    .Select(match => match.Closeness == null ? (MatchCloseness?)null : (MatchCloseness)Math.Max(0, Math.Ceiling(match.Closeness.Value) - 1))
                    .GroupBy(m => m)
                    .Select(gr => new { gr.Key, Count = gr.Count() })
                    .ToArray();

                var globalMatchesCloseness = generalTournamentStats.MatchesClosenessCount
                    .Select(gr => new { Key = (MatchCloseness)int.Parse(gr.Key), Count = gr.Value })
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

        private static ObjectResult WrapException(Exception ex)
            => new(ex.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

        private static (int Score, int OpponentScore) GetNormalizedMatchScore(Match match, string captainName)
        {
            if (NameExtensions.EqualsIgnoreCase(match.Team1CaptainName, captainName))
            {
                return (match.ScoreTeam1, match.ScoreTeam2);
            }
            else if (NameExtensions.EqualsIgnoreCase(match.Team2CaptainName, captainName))
            {
                return (match.ScoreTeam2, match.ScoreTeam1);
            }
            else
            {
                throw new ArgumentException($"Unexpected captain name ({captainName}) in the match", nameof(match));
            }
        }
    }
}
