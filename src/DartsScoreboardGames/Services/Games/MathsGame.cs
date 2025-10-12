namespace DartsScoreboardGames.Services.Games {
    public class MathsGame : IGameDefinition {
        public static string Name => "Sum Darts";
        public static string Description => "Select a Game";
        public static string Logo => "assets/dartboard-face-sums.png";
        public static string Background => "assets/darts-background-sums.png";
        public static int PlayerRequirement => 2;
    }
}
