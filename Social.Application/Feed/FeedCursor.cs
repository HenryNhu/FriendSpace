using System.Globalization;
using System.Text;

namespace Social.Application.Feed;

public sealed record FeedPosition(DateTimeOffset CreatedAt, Guid Id);

public static class FeedCursor
{
    public static string Encode(FeedPosition position)
    {
        var value = FormattableString.Invariant($"1:{position.CreatedAt.UtcTicks}:{position.Id:N}");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static FeedPosition? Decode(string? cursor)
    {
        if (cursor is null) return null;
        if (cursor.Length is < 1 or > 160 || cursor.Any(c =>
                !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw InvalidCursor();

        try
        {
            var base64 = cursor.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight((base64.Length + 3) / 4 * 4, '=');
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(base64)).Split(':');
            if (parts.Length != 3 || parts[0] != "1" ||
                !long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks) ||
                ticks < DateTimeOffset.MinValue.Ticks || ticks > DateTimeOffset.MaxValue.Ticks ||
                !Guid.TryParseExact(parts[2], "N", out var id) || id == Guid.Empty)
                throw InvalidCursor();

            return new FeedPosition(new DateTimeOffset(ticks, TimeSpan.Zero), id);
        }
        catch (FormatException)
        {
            throw InvalidCursor();
        }
    }

    private static ArgumentException InvalidCursor() =>
        new("Vị trí bảng tin không hợp lệ. Vui lòng tải lại bảng tin.");
}
