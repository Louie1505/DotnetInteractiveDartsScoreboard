namespace DartsScoreboardGames.Services.Games {
    public interface IGameState {
        public abstract static string Name { get; }
        public abstract static string Description { get; }
        public abstract static int PlayerRequirement { get; }
        public abstract static string Logo { get; }
        public abstract static string Background { get; }

        public abstract static IGameState Create(GameStateProvider gameStateProvider);

        public event EventHandler<EventArgs>? OnChange;
        public event EventHandler<EventArgs>? OnTurnEnd;
    }
}
