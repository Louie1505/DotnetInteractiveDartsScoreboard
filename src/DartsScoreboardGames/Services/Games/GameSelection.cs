namespace DartsScoreboardGames.Services.Games {
    public class GameSelection : IGameDefinition {
        public static string Name => "Game Select";
        public static string Description => "Select a Game";
        public static int PlayerRequirement => 1;
    }
}
