using Microsoft.Extensions.Logging;
using OWTournamentsHistory.DataAccess.Contract;
using System.Diagnostics;

namespace OWTournamentsHistory.Tasks
{
    public class CalculatePlayerCombinationsInvocable : BaseInvocable<CalculatePlayerCombinationsInvocable>
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerDuosRepository _playerDuosRepository;
        private readonly IPlayerOpponentsRepository _playerOpponentsRepository;

        public CalculatePlayerCombinationsInvocable(IMatchRepository matchRepository,
            ITeamRepository teamRepository,
            IPlayerDuosRepository playerDuosRepository,
            IPlayerOpponentsRepository playerOpponentsRepository,
            ILogger<CalculatePlayerCombinationsInvocable> logger)
            : base(logger)
        {
            _matchRepository = matchRepository;
            _teamRepository = teamRepository;
            _playerDuosRepository = playerDuosRepository;
            _playerOpponentsRepository = playerOpponentsRepository;
        }

        protected override async Task InvokeInternal()
        {
            var sw = new Stopwatch();
            sw.Start();
            var teamsWithPlayers = (await _teamRepository.GetAsync())
                .Select(team => new { team.TournamentNumber, team.CaptainName, Players = team.Players.Select(p => p.Name).ToArray() })
                .ToDictionary(team => (team.TournamentNumber, team.CaptainName.ToLowerInvariant()), team => team.Players);
            var allMatches = (await _matchRepository.GetAsync())
                .Where(match => teamsWithPlayers.ContainsKey((match.TournamentNumber, match.Team1CaptainName.ToLowerInvariant())) && teamsWithPlayers.ContainsKey((match.TournamentNumber, match.Team2CaptainName.ToLowerInvariant())))
                .Select(match => new { Match = match, PlayersTeam1 = teamsWithPlayers[(match.TournamentNumber, match.Team1CaptainName.ToLowerInvariant())], PlayersTeam2 = teamsWithPlayers[(match.TournamentNumber, match.Team2CaptainName.ToLowerInvariant())] })
                .ToArray();
            Debug.WriteLine($"Matches count: {allMatches.Length}");

            var scoreDuos = new Dictionary<(string Player1, string Player2), (int MapsWon, int MapsPlayed)>();
            var scoreOpponents = new Dictionary<(string Player1, string Player2), (int MapsWon, int MapsPlayed)>();

            foreach (var match in allMatches)
            {
                var team1Score = match.Match.ScoreTeam1;
                var team2Score = match.Match.ScoreTeam2;
                var totalScore = team1Score + team2Score;

                var duosTeam1 = FindAllPairs(match.PlayersTeam1);
                var duosTeam2 = FindAllPairs(match.PlayersTeam2);

                var opponents = FindCrossProduct(match.PlayersTeam1, match.PlayersTeam2);

                foreach (var duo in duosTeam1)
                {
                    if (scoreDuos.ContainsKey(duo))
                    {
                        var score = scoreDuos[duo];
                        score.MapsWon += team1Score;
                        score.MapsPlayed += totalScore;
                        scoreDuos[duo] = score;
                    }
                    else if (scoreDuos.ContainsKey(Reverse(duo)))
                    {
                        var score = scoreDuos[Reverse(duo)];
                        score.MapsWon += team1Score;
                        score.MapsPlayed += totalScore;
                        scoreDuos[Reverse(duo)] = score;
                    }
                    else
                    {
                        scoreDuos.Add(duo, (team1Score, totalScore));
                    }
                }

                foreach (var duo in duosTeam2)
                {
                    if (scoreDuos.ContainsKey(duo))
                    {
                        var score = scoreDuos[duo];
                        score.MapsWon += team2Score;
                        score.MapsPlayed += totalScore;
                        scoreDuos[duo] = score;
                    }
                    else if (scoreDuos.ContainsKey(Reverse(duo)))
                    {
                        var score = scoreDuos[Reverse(duo)];
                        score.MapsWon += team2Score;
                        score.MapsPlayed += totalScore;
                        scoreDuos[Reverse(duo)] = score;
                    }
                    else
                    {
                        scoreDuos.Add(duo, (team2Score, totalScore));
                    }
                }

                foreach (var opponent in opponents)
                {
                    if (scoreOpponents.ContainsKey(opponent))
                    {
                        var score = scoreOpponents[opponent];
                        score.MapsWon += team1Score;
                        score.MapsPlayed += totalScore;
                        scoreOpponents[opponent] = score;
                    }
                    else if (scoreOpponents.ContainsKey(Reverse(opponent)))
                    {
                        var score = scoreOpponents[Reverse(opponent)];
                        score.MapsWon += team2Score;
                        score.MapsPlayed += totalScore;
                        scoreOpponents[Reverse(opponent)] = score;
                    }
                    else
                    {
                        scoreOpponents.Add(opponent, (team1Score, totalScore));
                    }
                    //(Player1, Player2) -> (Won, TotalPlayed, Winrate)
                }
            }

            await _playerDuosRepository.Clear();
            await _playerOpponentsRepository.Clear();

            await _playerDuosRepository
                .AddRangeAsync(scoreDuos
                    .Select(duo => new DataAccess.Model.PlayerPair
                    {
                        Player1 = duo.Key.Player1,
                        Player2 = duo.Key.Player2,
                        MapsWon = duo.Value.MapsWon,
                        MapsPlayed = duo.Value.MapsPlayed
                    })
                    .ToArray());

            await _playerOpponentsRepository
                .AddRangeAsync(scoreOpponents
                    .Select(duo => new DataAccess.Model.PlayerPair
                    {
                        Player1 = duo.Key.Player1,
                        Player2 = duo.Key.Player2,
                        MapsWon = duo.Value.MapsWon,
                        MapsPlayed = duo.Value.MapsPlayed
                    })
                    .ToArray());

            sw.Stop();
            Debug.WriteLine(sw.Elapsed);
        }

        private static (T, T)[] FindAllPairs<T>(T[] source)
        {
            //count of pairs is a sum of an arithmetic progression:
            //2 - 1
            //3 - 3 -> 2 + 1
            //4 - 6 -> 3 + 2 + 1
            //5 - 10 -> 4 + 3 + 2 + 1
            //6 - 15 -> 5 + 4 + 3 + 2 + 1
            var a1 = source.Length - 1;
            var an = 1;
            var n = source.Length - 1;
            var sn = (a1 + an) * n / 2;
            var pairs = new (T, T)[sn];

            var counter = 0;
            for (var i = 0; i < source.Length; i++)
            {
                for (var j = i + 1; j < source.Length; j++)
                {
                    pairs[counter++] = (source[i], source[j]);
                }
            }

            return pairs;
        }

        private static (T, T)[] FindCrossProduct<T>(T[] source1, T[] source2)
        {
            var pairs = new (T, T)[source1.Length * source2.Length];
            var counter = 0;
            for (var i = 0; i < source1.Length; i++)
            {
                for (var j = 0; j < source2.Length; j++)
                {
                    pairs[counter++] = (source1[i], source2[j]);
                }
            }

            return pairs;
        }

        private static (T2, T1) Reverse<T1, T2>((T1, T2) value)
            => (value.Item2, value.Item1);
    }
}
