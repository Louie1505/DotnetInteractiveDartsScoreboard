using DartsScoreboardGames.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DartsScoreboardGames.Infrastructure.Data {
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {

        public DbSet<PlayerHistory> PlayerHistories => Set<PlayerHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
