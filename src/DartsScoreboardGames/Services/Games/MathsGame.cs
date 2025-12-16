namespace DartsScoreboardGames.Services.Games {
    public class MathsGame : IGameDefinition {
        public static string Name => "Sum Darts";
        public static string Description => "Finally, a game that combines darts and math—because nothing says fun like doing sums while aiming for triple 20.";
        public static string Logo => "assets/dartboard-face-sums.png";
        public static string Background => "assets/darts-background-sums.png";
        public static int PlayerRequirement => 2;
    }
}
