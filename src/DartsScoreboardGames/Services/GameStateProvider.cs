using DartsScoreboardGames.Services.Games;
using DartsScoreboardGames.Services.Models;
using System.Collections.ObjectModel;

namespace DartsScoreboardGames.Services {
    public class GameStateProvider {

        public IGameDefinition? CurrentGame { get; private set; }

        private List<Player> _players { get; } = [];
        public ReadOnlyCollection<Player> Players => _players.AsReadOnly();

        public void AddPlayer(Player player) {
            if (!Players.Contains(player)) {
                _players.Add(player);
            }
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemovePlayer(Player player) {
            _players.Remove(player);
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void EndGame() {
            CurrentGame = null;
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetGameScreen<TGame>() where TGame : IGameDefinition, new() {
            CurrentGame = new TGame();
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs>? OnGameStateChanged;



    }
}
