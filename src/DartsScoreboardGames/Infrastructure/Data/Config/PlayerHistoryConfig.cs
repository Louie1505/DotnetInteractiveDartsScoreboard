using DartsScoreboardGames.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DartsScoreboardGames.Infrastructure.Data.Config;
public class PlayerHistoryConfig : IEntityTypeConfiguration<PlayerHistory> {
    public void Configure(EntityTypeBuilder<PlayerHistory> builder) {
        builder.ToTable("PlayerHistory");

        builder.HasKey(x => x.Id);

        builder.ComplexProperty(x => x.Player);
    }
}
