using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace SongList.Web;

public class AppContext : DbContext
{
    public AppContext(DbContextOptions options): base(options)
    {   
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonSerializerOptions = new JsonSerializerOptions();
        modelBuilder.Entity<Song>()
            .Property(x => x.Tags)
            .HasConversion(
                convertToProviderExpression: value => JsonSerializer.Serialize(value, jsonSerializerOptions) ?? "[]",
                convertFromProviderExpression: value =>
                    JsonSerializer.Deserialize<string[]>(value ?? "[]", jsonSerializerOptions) ?? Array.Empty<string>())
            .HasMaxLength(64 * 1024)
            .IsRequired(false)
            .HasColumnType("jsonb");
        base.OnModelCreating(modelBuilder);
    }
}