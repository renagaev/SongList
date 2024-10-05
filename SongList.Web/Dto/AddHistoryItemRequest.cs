namespace SongList.Web.Dto;

public record AddHistoryItemRequest(string HolyricsId, DateTimeOffset CreatedAt, string Title);