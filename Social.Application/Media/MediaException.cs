namespace Social.Application.Media;

public sealed class MediaException(string code, string message, int statusCode = 400,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
