using DiffPlex.Chunkers;
using Microsoft.Extensions.Options;
using SongList.Web.Auth;
using SongList.Web.Dto;
using Telegram.Bot;
using System.Net;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.EntityFrameworkCore;
using SongList.Web.UseCases.SyncHolyricsSongs;
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
            var diff = DiffTextBlockHtml(before.Text ?? "", after.Text ?? "");
            changes.Add($"• Текст:\n<blockquote expandable>{diff}</blockquote>");
        }

        if (changes.Count > 0)
        {
            var header =
                $"<b>{EscapeHtml(userName)}</b> обновил песню “<b>{EscapeHtml(after.Title ?? "")}</b>”";
            var message = header + "\n" + string.Join("\n", changes);

            await botClient.SendMessage(
                chatId: options.Value.ChatId,
                messageThreadId: options.Value.UpdatesThreadId,
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task NotifyNewSongsImported(ICollection<string> titles, CancellationToken cancellationToken)
    {
        var message = $"Добавлены новые песни из Holyrics\n\n{string.Join("\n", titles)}";
        await botClient.SendMessage(
            chatId: options.Value.ChatId,
            text: message,
            messageThreadId: options.Value.UpdatesThreadId,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public async Task NotifyHolyricsUpdate(SyncSong before, SyncSong after, CancellationToken cancellationToken)
    {
        var changes = new List<string>();

        if (!string.Equals(before.Title, after.Title, StringComparison.Ordinal))
            changes.Add($"• Название: {DiffInlineHtml(before.Title ?? "", after.Title ?? "")}");

        if (before.Number != after.Number)
            changes.Add(
                $"• Номер: <b>{EscapeHtml(before.Number?.ToString() ?? "—")}</b> → <b>{EscapeHtml(after.Number?.ToString() ?? "—")}</b>");
        

        if (before.Note != after.Note)
        {
            changes.Add($"• Нота: <b>{EscapeHtml(before.Note ?? "-")}</b> → <b>{EscapeHtml(after.Note ?? "-")}</b>");
        }

        if (!string.Equals(before.Text, after.Text, StringComparison.Ordinal))
        {
            var diff = DiffTextBlockHtml(before.Text ?? "", after.Text ?? "");
            changes.Add($"• Текст:\n<blockquote expandable>{diff}</blockquote>");
        }

        if (changes.Count > 0)
        {
            var header =
                $"<b>Обновлена песня Holyrics</b>“<b>{EscapeHtml(after.Title ?? "")}</b>”";
            var message = header + "\n" + string.Join("\n", changes);

            await botClient.SendMessage(
                chatId: options.Value.ChatId,
                messageThreadId: options.Value.HolyricsUpdatesThreadId,
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task NotifySongDeleted(string title, string userName, CancellationToken cancellationToken)
    {
        var message =
            $"<b>{EscapeHtml(userName)}</b> удалил песню “<b>{EscapeHtml(title ?? "")}</b>”";

        await botClient.SendMessage(
            chatId: options.Value.ChatId,
            messageThreadId: options.Value.UpdatesThreadId,
            text: message,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
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
            var isWhitespaceOnly = string.IsNullOrWhiteSpace(line.Text);

            switch (line.Type)
            {
                case ChangeType.Inserted:
                    parts.Add(isWhitespaceOnly ? text : $"<b>{text}</b>");
                    break;
                case ChangeType.Deleted:
                    parts.Add(isWhitespaceOnly ? text : $"<s>{text}</s>");
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

    private const int ContextLines = 1;

    private static string DiffTextBlockHtml(string oldText, string newText)
    {
        var lineModel = InlineDiffBuilder.Diff(
            oldText,
            newText,
            ignoreWhiteSpace: true,
            ignoreCase: false,
            chunker: new LineChunker()
        );

        var lines = lineModel.Lines;
        var output = new List<string>();
        var i = 0;

        while (i < lines.Count)
        {
            if (lines[i].Type == ChangeType.Unchanged)
            {
                var runStart = i;
                while (i < lines.Count && lines[i].Type == ChangeType.Unchanged) i++;

                EmitUnchangedRun(output, lines, runStart, i, keepStart: runStart > 0, keepEnd: i < lines.Count);
                continue;
            }

            var deletedRun = new List<string>();
            while (i < lines.Count && lines[i].Type == ChangeType.Deleted)
            {
                deletedRun.Add(lines[i].Text);
                i++;
            }

            var insertedRun = new List<string>();
            while (i < lines.Count && lines[i].Type == ChangeType.Inserted)
            {
                insertedRun.Add(lines[i].Text);
                i++;
            }

            var pairCount = Math.Min(deletedRun.Count, insertedRun.Count);
            for (var p = 0; p < pairCount; p++)
            {
                var (oldHtml, newHtml) = DiffLinePairHtml(deletedRun[p], insertedRun[p]);
                output.Add("➖ " + oldHtml);
                output.Add("➕ " + newHtml);
            }

            for (var p = pairCount; p < deletedRun.Count; p++)
                output.Add("➖ <s>" + EscapeHtml(deletedRun[p]) + "</s>");

            for (var p = pairCount; p < insertedRun.Count; p++)
                output.Add("➕ <b>" + EscapeHtml(insertedRun[p]) + "</b>");
        }

        return string.Join("\n", output);
    }

    private static void EmitUnchangedRun(List<string> output, List<DiffPiece> lines, int start, int end,
        bool keepStart, bool keepEnd)
    {
        var count = end - start;
        var headCount = keepStart ? ContextLines : 0;
        var tailCount = keepEnd ? ContextLines : 0;

        if (count <= headCount + tailCount + 1)
        {
            for (var k = start; k < end; k++) output.Add(EscapeHtml(lines[k].Text));
            return;
        }

        for (var k = start; k < start + headCount; k++) output.Add(EscapeHtml(lines[k].Text));

        var hidden = count - headCount - tailCount;
        output.Add($"<i>… {hidden} {RussianLinesWord(hidden)} без изменений …</i>");

        for (var k = end - tailCount; k < end; k++) output.Add(EscapeHtml(lines[k].Text));
    }

    private static string RussianLinesWord(int n)
    {
        var n100 = n % 100;
        if (n100 is >= 11 and <= 14) return "строк";
        return (n % 10) switch
        {
            1 => "строка",
            >= 2 and <= 4 => "строки",
            _ => "строк"
        };
    }

    private static (string OldHtml, string NewHtml) DiffLinePairHtml(string oldLine, string newLine)
    {
        var model = InlineDiffBuilder.Diff(
            oldLine,
            newLine,
            ignoreWhiteSpace: true,
            ignoreCase: false,
            chunker: new WordChunker()
        );

        var oldParts = new List<string>();
        var newParts = new List<string>();

        foreach (var piece in model.Lines)
        {
            var text = EscapeHtml(piece.Text);
            var isWhitespaceOnly = string.IsNullOrWhiteSpace(piece.Text);

            switch (piece.Type)
            {
                case ChangeType.Unchanged:
                    oldParts.Add(text);
                    newParts.Add(text);
                    break;
                case ChangeType.Deleted:
                    oldParts.Add(isWhitespaceOnly ? text : $"<s>{text}</s>");
                    break;
                case ChangeType.Inserted:
                    newParts.Add(isWhitespaceOnly ? text : $"<b>{text}</b>");
                    break;
            }
        }

        return (string.Concat(oldParts), string.Concat(newParts));
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
