using DartsScoreboardGames.Services.Games;
using DartsScoreboardGames.Services.Models;
using System.Collections.ObjectModel;

namespace DartsScoreboardGames.Services {
    public class GameStateProvider {

        public IGameState? CurrentGame { get; private set; }

        private List<Player> _players { get; } = [];
        private List<Player> _playersOut { get; } = [];

        public ReadOnlyCollection<Player> Players => _players.AsReadOnly();
        public Player ActivePlayer = default!;

        private Dictionary<Player, List<Turn>> turns = [];
        public Turn? CurrentTurn => turns.TryGetValue(ActivePlayer, out List<Turn>? playerTurns) ? playerTurns.LastOrDefault() : default;
        public List<Turn> PlayerTurns(Player player) => turns[player];

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
            _playersOut.Clear();
            CurrentGame = null;
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetGameScreen<TGame>() where TGame : IGameState, new() {
            _playersOut.Clear();
            CurrentGame = TGame.Create(this);
            ActivePlayer = Players.First();
            turns = Players.ToDictionary(x => x, x => new List<Turn>() { new() });
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void MarkPlayerOut(Player player) =>
            _playersOut.Add(player);

        public void SetNextPlayer() {
            turns[ActivePlayer].Add(new());

            ActivePlayer = Players.IndexOf(ActivePlayer) == Players.Count - 1 ?
                Players.First() :
                Players.ElementAt(Players.IndexOf(ActivePlayer) + 1);

            if (_playersOut.Contains(ActivePlayer)) {
                SetNextPlayer();

            }

        }

        public void ForceStateChange(){ 
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task CurrentPlayerWin() =>
            await Task.Run(() => OnPlayerWin?.Invoke(this, new() { WinningPlayer = ActivePlayer }));

        public event EventHandler<EventArgs>? OnGameStateChanged;
        public event EventHandler<PlayerWinEventArgs>? OnPlayerWin;


        public class PlayerWinEventArgs : EventArgs {
            public required Player WinningPlayer { get; set; }
        }


    }
}
