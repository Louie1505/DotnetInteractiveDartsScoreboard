using DartsScoreboardGames.Services.Models;

namespace DartsScoreboardGames.Infrastructure.Models {
    public class PlayerHistory {
        public int Id { get; set; }
        public Player Player { get; set; } = default!;
        public DateTime DateCreated  { get; set; } = DateTime.UtcNow;
    }
}
