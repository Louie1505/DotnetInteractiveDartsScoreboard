namespace DartsScoreboardGames.Services.Games {
    public class KillerGame : IGameDefinition {
        public static string Name => "Killer";
        public static string Description => "Select a Game";
        public static string Logo => "assets/dartboard-face-killer.png";
        public static int PlayerRequirement => 2;
    }
}
