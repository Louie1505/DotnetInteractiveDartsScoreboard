using System.Collections.ObjectModel;

namespace DartsScoreboardGames.Services {

    public record Game;
    public record Player(string Name);

    public class GameStateProvider {

        public Game? CurrentGame { get; set; }

        private List<Player> _players { get; } = [];
        public ReadOnlyCollection<Player> Players => _players.AsReadOnly();

        public void AddPlayer(Player player) { 
            _players.Add(player);
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs>? OnGameStateChanged;
 


    }
}
