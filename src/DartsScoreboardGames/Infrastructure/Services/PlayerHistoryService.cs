using DartsScoreboardGames.Infrastructure.Data;
using DartsScoreboardGames.Infrastructure.Models;
using DartsScoreboardGames.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace DartsScoreboardGames.Infrastructure.Services {
    public class PlayerHistoryService {

        private readonly AppDbContext _appDbContext;

        public PlayerHistoryService(AppDbContext appDbContext) {
            _appDbContext = appDbContext;
        }

        public async Task AddPlayerHistoryAsync(Player player) {
            await RemovePlayerFromHistoryAsync(player);

            _appDbContext
                .PlayerHistories
                .Add(new PlayerHistory() {
                    Player = player
                });

            await _appDbContext.SaveChangesAsync();
        }

        public async Task RemovePlayerFromHistoryAsync(Player player) {
            var existing = await _appDbContext
                .PlayerHistories
                .Where(x => x.Player == player)
                .ToListAsync();

            _appDbContext
                .PlayerHistories
                .RemoveRange(existing);

            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<Player>> GetHistoricPlayersAsync() =>
            await _appDbContext
                .PlayerHistories
                .OrderByDescending(x => x.DateCreated)
                .Select(x => x.Player)
                .ToListAsync();
    }
}
