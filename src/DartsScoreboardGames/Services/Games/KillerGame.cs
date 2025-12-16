namespace DartsScoreboardGames.Services.Games {
    public class KillerGame : IGameDefinition {
        public static string Name => "Killer";
        public static string Description => "Killer: where friendships are tested, alliances are broken, and only one player walks away victorious (and maybe still friends).";
        public static string Logo => "assets/dartboard-face-killer.png";
        public static string Background => "assets/darts-background-killer.png";
        public static int PlayerRequirement => 2;
    }
}
