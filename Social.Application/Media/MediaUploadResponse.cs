namespace Social.Application.Media;

public sealed record MediaUploadResponse(Guid Id, string Url);
public sealed record MediaDownload(Stream Content, string ContentType);
