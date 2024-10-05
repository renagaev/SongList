namespace HolyricsCompanion.Workers;

public record WorkersSettings
{
    public TimeSpan OutboxInterval { get; init; }
    public TimeSpan HistoryPollingInterval { get; init; }
}