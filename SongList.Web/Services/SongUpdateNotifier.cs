using DiffPlex.Chunkers;
using Microsoft.Extensions.Options;
using SongList.Web.Auth;
using SongList.Web.Dto;
using Telegram.Bot;
using System.Net;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace SongList.Web.Services;

public class SongUpdateNotifier(AppContext appContext, ITelegramBotClient botClient, IOptions<TgOptions> options)
{
    public async Task NofifyUpdate(SongDto before, SongDto after, string userName, CancellationToken cancellationToken)
    {
        var changes = new List<string>();

        if (!string.Equals(before.Title, after.Title, StringComparison.Ordinal))
            changes.Add($"• Название: {DiffInlineHtml(before.Title ?? "", after.Title ?? "")}");

        if (before.Number != after.Number)
            changes.Add(
                $"• Номер: <b>{EscapeHtml(before.Number?.ToString() ?? "—")}</b> → <b>{EscapeHtml(after.Number?.ToString() ?? "—")}</b>");

        if (!SequenceEqualIgnoreOrder(before.Tags, after.Tags))
            changes.Add($"• Теги: {DiffTagsHtml(before.Tags, after.Tags)}");

        if (before.NoteId != after.NoteId)
        {
            var oldNoteName = await GetNoteNameSafe(before.NoteId, cancellationToken);
            var newNoteName = await GetNoteNameSafe(after.NoteId, cancellationToken);
            changes.Add($"• Нота: <b>{EscapeHtml(oldNoteName)}</b> → <b>{EscapeHtml(newNoteName)}</b>");
        }

        if (!string.Equals(before.Text, after.Text, StringComparison.Ordinal))
        {
            var diff = DiffInlineHtml(before.Text ?? "", after.Text ?? "");
            changes.Add($"• Текст:\n<blockquote expandable>{diff}</blockquote>");
        }

        if (changes.Count > 0)
        {
            var header =
                $"<b>{EscapeHtml(userName)}</b> обновил песню “<b>{EscapeHtml(after.Title ?? "")}</b>”";
            var message = header + "\n" + string.Join("\n", changes);

            await botClient.SendMessage(
                chatId: options.Value.ChatId,
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
    }

    private static string EscapeHtml(string value) =>
        WebUtility.HtmlEncode(value ?? "");

    private static string DiffInlineHtml(string oldText, string newText)
    {
        var model = InlineDiffBuilder.Diff(
            oldText,
            newText,
            ignoreWhiteSpace: true,
            ignoreCase: false,
            chunker: new WordChunker()
        );

        var parts = new List<string>(model.Lines.Count);

        foreach (var line in model.Lines)
        {
            var text = EscapeHtml(line.Text);

            switch (line.Type)
            {
                case ChangeType.Inserted:
                    parts.Add($"<b>{text}</b>");
                    break;
                case ChangeType.Deleted:
                    parts.Add($"<s>{text}</s>");
                    break;
                case ChangeType.Unchanged:
                    parts.Add(text);
                    break;
                case ChangeType.Modified:
                default:
                    parts.Add(text);
                    break;
            }
        }

        return string.Concat(parts);
    }

    private static bool SequenceEqualIgnoreOrder(string[]? a, string[]? b)
    {
        a ??= [];
        b ??= [];
        return a.ToHashSet().SetEquals(b);
    }

    private static string DiffTagsHtml(string[]? oldTags, string[]? newTags)
    {
        oldTags ??= [];
        newTags ??= [];

        var oldSet = new HashSet<string>(oldTags, StringComparer.OrdinalIgnoreCase);

        var newSet = new HashSet<string>(newTags, StringComparer.OrdinalIgnoreCase
        );

        var kept = newSet.Intersect(oldSet).ToArray();
        var added = newSet.Except(oldSet).ToArray();
        var removed = oldSet.Except(newSet).ToArray();

        var chunks = new List<string>();

        if (kept.Length > 0)
            chunks.Add(EscapeHtml(string.Join(", ", kept)));

        if (added.Length > 0)
            chunks.Add("<b>+" + EscapeHtml(string.Join(", ", added)) + "</b>");

        if (removed.Length > 0)
            chunks.Add("<s>-" + EscapeHtml(string.Join(", ", removed)) + "</s>");

        return chunks.Count == 0 ? "—" : string.Join(" ", chunks);
    }

    private async Task<string> GetNoteNameSafe(int? noteId, CancellationToken ct)
    {
        if (noteId is null) return "—";

        var note = await appContext.Notes.FirstOrDefaultAsync(x => x.Id == noteId.Value, ct);
        return note.DetailedName;
    }
}