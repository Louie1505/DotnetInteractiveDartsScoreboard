
namespace DartsScoreboardGames.Services.Games {
    public class GameSelection : IGameState {
        public static string Name => "Game Select";
        public static string Description => "Select a Game";
        public static string Logo => "None";
        public static string Background => "None";
        public static int PlayerRequirement => 1;

        public event EventHandler<EventArgs>? OnChange;
        public event EventHandler<EventArgs>? OnTurnEnd;

        public static IGameState Create(GameStateProvider gameStateProvider) =>
            new GameSelection();
    }
}
