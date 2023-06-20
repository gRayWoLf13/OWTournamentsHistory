using Microsoft.Extensions.Logging;
using OWTournamentsHistory.Common.Utils;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Model;
using OWTournamentsHistory.DataAccess.Model.Statistics;
using OWTournamentsHistory.DataAccess.Model.Type;

namespace OWTournamentsHistory.Tasks
{
    public class CalculateGeneralTournamentStatsInvocable : BaseInvocable<CalculateGeneralTournamentStatsInvocable>
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IGeneralTournamentStatsRepository _generalTournamentStatsRepository;
        public CalculateGeneralTournamentStatsInvocable(
            IMatchRepository matchRepository,
            IPlayerRepository playerRepository,
            ITeamRepository teamRepository,
            IGeneralTournamentStatsRepository generalTournamentStatsRepository,
            ILogger<CalculateGeneralTournamentStatsInvocable> logger)
            : base(logger)
        {
            _matchRepository = matchRepository;
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
            _generalTournamentStatsRepository = generalTournamentStatsRepository;
        }

        protected override async Task InvokeInternal()
        {
            var allTeams = await _teamRepository.GetAsync();
            var allMatches = await _matchRepository.GetAsync();
            var allPlayers = await _playerRepository.GetAsync();

            var matchesWithPlayers = allMatches.Select(match => new
            {
                Match = match,
                PlayersTeam1 = allTeams.Single(team => team.TournamentNumber == match.TournamentNumber && team.CaptainName == match.Team1CaptainName).Players,
                PlayersTeam2 = allTeams.Single(team => team.TournamentNumber == match.TournamentNumber && team.CaptainName == match.Team2CaptainName).Players,
            });

            var tournamentsCount = allTeams
                .Select(team => team.TournamentNumber)
                .Distinct()
                .Count();

            var playersToDivisionsCount = allTeams
                              .SelectMany(p => p.Players)
                              .GroupBy(p => p.Division)
                              .Where(gr => gr.Key is not null)
                              .OrderBy(gr => gr.Key)
                              .ToDictionary(gr => (gr.Key ?? 0).ToString(), gr => gr.Count());

            var matchesClosenessCount = allMatches
                .Where(match => match.Closeness is not null)
                .Select(match => (int)Math.Max(0, Math.Ceiling(match.Closeness!.Value) - 1))
                .GroupBy(m => m)
                .OrderBy(gr => gr.Key)
                .ToDictionary(gr => gr.Key.ToString(), gr => gr.Count());

            var championsCount = allTeams.Where(team => team.Place == 1).Select(team => team.Players.Count).Sum();

            var allTeamPlayers = allTeams
                .SelectMany(team => team.Players.Select(p => new { Player = p, team.MapsWon, team.MapsPlayed }).ToArray()).ToArray();
            var roleWinRates = allTeamPlayers
                .GroupBy(player => player.Player.Role)
                .Select(gr => new GeneralRoleWinRateInfo { Role = gr.Key, AverageWinRate = gr.Average(player => player.MapsWon / (decimal)player.MapsPlayed) })
                .ToArray();

            var averageWinRate = allTeamPlayers.Average(player => player.MapsWon / (decimal)player.MapsPlayed);
            var averageTournamentsPlayedPercentage = allTeamPlayers
                    .GroupBy(p => p.Player.Name)
                    .Average(gr => (decimal)gr.Count())
                     / tournamentsCount;

            var averageMatchClosenessToTournament = allMatches
                .GroupBy(match => match.TournamentNumber)
                .Select(gr => (TournamentNumber: gr.Key, Closeness: gr.Average(match => match.Closeness ?? 0)));

            var tournamentTeamsStats = allTeams
                .GroupBy(team => team.TournamentNumber)
                .Select(gr => (TournamentNumber: gr.Key, PlayersCount: gr.Sum(team => team.Players.Count), TeamWeight: gr.Average(team => team.Players.Average(player => player.Weight ?? 0))))
                .ToArray();

            var averageDivisionToTournament = allTeams
               .GroupBy(team => team.TournamentNumber)
               .Select(gr => (TournamentNumber: gr.Key,
                   Tank: (decimal?)gr.SelectMany(team => team.Players.Where(player => player.Role == TeamPlayerRole.Tank)).Average(player => player.Division),
                   Dps: (decimal?)gr.SelectMany(team => team.Players.Where(player => player.Role == TeamPlayerRole.Dps)).Average(player => player.Division),
                   Support: (decimal?)gr.SelectMany(team => team.Players.Where(player => player.Role == TeamPlayerRole.Support)).Average(player => player.Division)))
               .ToArray();

            var topPlayersByScore = (from match in allMatches
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
                  .Take(20)
                  .ToDictionary(item => item.PlayerName, item => new GeneralTournamentChampionInfo { ChampionsCount = item.Roles.Length, Roles = item.Roles });



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
                .Take(20)
                .ToDictionary(item => item.PlayerName, item => new GeneralTournamentChampionInfo { ChampionsCount = item.Roles.Length, Roles = item.Roles });

            var playerRolesToDivisions = allTeams
                  .SelectMany(team => team.Players.Where(player => player.Division is not null).Select(player => (Division: (int)player.Division, player.Role)))
                  .GroupBy(player => player.Division)
                  .ToDictionary(gr => gr.Key.ToString(), gr => new GeneralRolesToDivisionsInfo
                  {
                      TanksCount = gr.Count(player => player.Role == TeamPlayerRole.Tank),
                      DpsCount = gr.Count(player => player.Role == TeamPlayerRole.Dps),
                      SupportsCount = gr.Count(player => player.Role == TeamPlayerRole.Support),
                      FlexCount = gr.Count(player => player.Role == TeamPlayerRole.Flex)
                  });

            //stats to tournaments calculation
            var statsToTournaments = new Dictionary<string, TournamentStats>();
            foreach(var (tournamentNumber, tournamentTeams) in allTeams.GroupBy(team => team.TournamentNumber).Select(gr => (TournamentNumber: gr.Key, TournamentTeams: gr.ToArray())))
            {
                var tournamentStats = new TournamentStats
                {
                    AverageCloseness = tournamentTeams.Where(team => team.AverageMatchesCloseScore is not null).Average(team => team.AverageMatchesCloseScore) ?? 0,
                    TeamsCount = tournamentTeams.Length,
                    PlayersCount = tournamentTeams.Sum(team => team.Players.Count),
                    MatchesCount = allMatches.Count(match => match.TournamentNumber == tournamentNumber),
                    AverageTeamWeight = tournamentTeams.Average(team => team.Players.Average(player => player.Weight ?? 0)),
                    AverageDivisionToRole =
                    new[]
                    {
                        new DivisionToRoleInfo { Role = TeamPlayerRole.Tank, Division = (decimal?)tournamentTeams.SelectMany(team => team.Players.Where(player => player.Role == TeamPlayerRole.Tank)).Average(player => player.Division) },
                        new DivisionToRoleInfo { Role = TeamPlayerRole.Dps, Division = (decimal?)tournamentTeams.SelectMany(team => team.Players.Where(player => player.Role == TeamPlayerRole.Dps)).Average(player => player.Division) },
                        new DivisionToRoleInfo { Role = TeamPlayerRole.Support, Division = (decimal?)tournamentTeams.SelectMany(team => team.Players.Where(player => player.Role == TeamPlayerRole.Support)).Average(player => player.Division) }
                    }
                };
                statsToTournaments.Add(tournamentNumber.ToString(), tournamentStats);
            }

            var generalTournamentStats = new GeneralTournamentStats
            {
                TournamentsCount = tournamentsCount,
                OWALsCount = 1,
                ChampionsCount = championsCount,
                AverageWinRate = averageWinRate,
                AverageTournamentsPlayedPercentage = averageTournamentsPlayedPercentage,

                RoleWinRates = roleWinRates,

                MatchesClosenessCount = matchesClosenessCount,
                PlayerRolesToDivisions = playerRolesToDivisions,
                PlayersToDivisionsCount = playersToDivisionsCount,

                StatsToTournaments = statsToTournaments,

                Top0Wins = top0Wins,
                TopChampions = topChampions,
                TopPlayersByScore = topPlayersByScore.ToDictionary(item => item.PlayerName, item => item.Score),
                TopWinRate = topWinRate.ToDictionary(item => item.PlayerName, item => item.WinRate),

                LastModified = DateTimeOffset.UtcNow
            };

            await _generalTournamentStatsRepository.UpdateStats(generalTournamentStats);
        }
    }
}
