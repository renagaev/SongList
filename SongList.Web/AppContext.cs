using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SongList.Web.Entities;

namespace SongList.Web;

public class AppContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Song> Songs { get; init; }
    public DbSet<SongHistoryItem> History { get; init; }
    public DbSet<SongMapping> Mappings { get; init; }
    public DbSet<Note> Notes { get; init; }
    public DbSet<SongAttachment> Attachments { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonSerializerOptions = new JsonSerializerOptions();
        modelBuilder.Entity<Song>()
            .ToTable("Song")
            .Property(x => x.Tags)
            .HasConversion(
                convertToProviderExpression: value => JsonSerializer.Serialize(value, jsonSerializerOptions),
                convertFromProviderExpression: value =>
                    JsonSerializer.Deserialize<string[]>(value, jsonSerializerOptions) ?? Array.Empty<string>())
            .HasMaxLength(64 * 1024)
            .IsRequired(false)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Song>()
            .HasOne(x => x.Note);

        modelBuilder.Entity<SongHistoryItem>().HasIndex(x => x.SongId);
        modelBuilder.Entity<SongMapping>().HasIndex(x => x.HolyricsId);
        modelBuilder.Entity<SongMapping>().HasIndex(x => new { x.SongId, x.HolyricsId }).IsUnique();


        modelBuilder.Entity<Song>()
            .HasMany(x => x.Attachments)
            .WithOne(x => x.Song);

        modelBuilder.Entity<SongAttachment>()
            .HasQueryFilter(x => !x.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}