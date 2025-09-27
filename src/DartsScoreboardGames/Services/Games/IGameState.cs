namespace DartsScoreboardGames.Services.Games {
    public interface IGameDefinition {
        public abstract static string Name { get; }
        public abstract static string Description { get; }
        public abstract static int PlayerRequirement { get; }
        public abstract static string Logo { get; }
    }
}
