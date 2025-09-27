namespace DartsScoreboardGames.Services.Games {
    public class StandardGame : IGameDefinition {
        public static string Name => "Standard Game";
        public static string Description => "Select a Game";
        public static int PlayerRequirement => 1;
    }
}
