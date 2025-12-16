
using DartsScoreboardGames.Services.Models;

namespace DartsScoreboardGames.Services.Games {
    public class KillerGame : IGameState {
        public static string Name => "Killer";
        public static string Description => "Killer: where friendships are tested, alliances are broken, and only one player walks away victorious (and maybe still friends).";
        public static string Logo => "assets/dartboard-face-killer.png";
        public static string Background => "assets/darts-background-killer.png";
        public static int PlayerRequirement => 2;

        public event EventHandler<EventArgs>? OnChange;
        public event EventHandler<EventArgs>? OnTurnEnd;


        private Dictionary<Player, int> playerNumbers = [];
        private Dictionary<Player, int> playerScores = [];
        private GameStateProvider GameStateProvider { get; set; } = default!;

        public static IGameState Create(GameStateProvider gameStateProvider) {
            var game = new KillerGame() {
                GameStateProvider = gameStateProvider,
                playerScores = gameStateProvider.Players.ToDictionary(x => x, x => 0)
            };

            foreach (var player in gameStateProvider.Players) {
                int number = Random.Shared.Next(1, 21);
                while (game.playerNumbers.ContainsValue(number)) {
                    number = Random.Shared.Next(1, 21);
                }
                game.playerNumbers.Add(player, number);

            }

            return game;
        }

        public async Task OnDart(Dart dart) {
            GameStateProvider.CurrentTurn?.AddNext(dart);

            //here
            if (PlayerIsKiller(GameStateProvider.ActivePlayer)) {
               var hitPlayer=PlayerWithNumber(dart.FaceValue);
                if(hitPlayer is not null && hitPlayer!=GameStateProvider.ActivePlayer) {
                     var score = 1;

                    if(dart.IsDouble)
                        score *=2;

                    if(dart.IsTreble)
                        score *=3;

                    playerScores[hitPlayer] = Math.Max(playerScores[hitPlayer] - score, -1);
                    if(playerScores [hitPlayer] <0) {
                        GameStateProvider.MarkPlayerOut(hitPlayer);
                        if(playerScores.Where(x => x.Key != GameStateProvider.ActivePlayer).All(x => x.Value < 0)) {
                            await GameStateProvider.CurrentPlayerWin();
                        }
                    }

                }
            } else {
                if (dart.FaceValue == NumberForPlayer(GameStateProvider.ActivePlayer)) {
                    var score = 1;

                    if(dart.IsDouble)
                        score *=2;

                    if(dart.IsTreble)
                        score *=3;

                    playerScores[GameStateProvider.ActivePlayer] = Math.Min(playerScores[GameStateProvider.ActivePlayer] + score, 3);
                }

            }

            if ((GameStateProvider.CurrentTurn?.Complete ?? false)) {
                await EndTurn();
            }
        }

        public bool PlayerIsDead(Player player) =>
            PlayerScore(player) < 0;

        public bool PlayerIsKiller(Player player) =>
            PlayerScore(player) == 3;

        public Player? PlayerWithNumber(int num) =>
            playerNumbers.ContainsValue(num) ? playerNumbers.First(x => x.Value == num).Key : null;

        public int NumberForPlayer(Player player) =>
            playerNumbers[player];

        public int PlayerScore(Player player) =>
           playerScores[player];

        public async Task EndTurn() {
            GameStateProvider.CurrentTurn?.End();
            GameStateProvider.ForceStateChange();
            await Task.Delay(1000);
            GameStateProvider.SetNextPlayer();
            OnTurnEnd?.Invoke(this, EventArgs.Empty);
        }
    }
}
