using DartsScoreboardGames.Components.Pages.Screens.SharedComponents;
using DartsScoreboardGames.Services.Models;
using MudBlazor;

namespace DartsScoreboardGames.Services.Games {
    public class StandardGame : IGameState {
        public static string Name => "Standard";
        public static string Description => "Just your classic game of darts—because who needs excitement when you have tradition?";
        public static string Logo => "assets/dartboard-face-standard.png";
        public static string Background => "assets/darts-background-standard.png";
        public static int PlayerRequirement => 1;
        public static IGameState Create(GameStateProvider gameStateProvider) {
            return new StandardGame() {
                GameStateProvider = gameStateProvider,
                scores = gameStateProvider.Players.ToDictionary(x => x, x => 301),
            };
        }




        private Dictionary<Player, int> scores = [];

        public event EventHandler<EventArgs>? OnTurnEnd;
        public event EventHandler<EventArgs>? OnChange;

        private GameStateProvider GameStateProvider { get; set; } = default!;

        public async Task OnDart(Dart dart) {

            if (ActivePlayerScore() - dart.Value <= 1) {
                if ((dart.IsDouble || dart == Dart.Bull) && dart.Value == ActivePlayerScore()) {
                    await GameStateProvider.CurrentPlayerWin();
                } else {
                    GameStateProvider.CurrentTurn?.GoBust();
                }
            } else {
                GameStateProvider.CurrentTurn?.AddNext(dart);
                scores[GameStateProvider.ActivePlayer] -= dart.Value;
                OnChange?.Invoke(this, EventArgs.Empty);
            }

            if ((GameStateProvider.CurrentTurn?.Complete ?? false) || (GameStateProvider.CurrentTurn?.Bust ?? false)) {
                await EndTurn();
            }
        }

        public int ActivePlayerScore() =>
            scores[GameStateProvider.ActivePlayer];

        public int PlayerScore(Player player) =>
            scores[player];

        public async Task EndTurn() {
            GameStateProvider.CurrentTurn?.End();
            GameStateProvider.ForceStateChange();
            await Task.Delay(1000);
            GameStateProvider.SetNextPlayer();
            OnTurnEnd?.Invoke(this, EventArgs.Empty);
        }

        public double AvgScore(Player player) {
            var turns = GameStateProvider.PlayerTurns(player);
            var completed = turns.Where(x => x.Complete);

            if (!completed.Any()) {
                return 0;
            }

            return Math.Round(completed.Average(x => x.TotalValue), 2);
        }

        public double HighestScore(Player player) {
            var turns = GameStateProvider.PlayerTurns(player);

            var completed = turns.Where(x => x.Complete);

            if (!completed.Any()) {
                return 0;
            }

            return completed.Max(x => x.TotalValue);
        }
    }
}
