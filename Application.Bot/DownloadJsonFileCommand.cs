using MediatR;

namespace Application.Bot;

public record DownloadJsonFileCommand : IRequest
{
    public long SessionId { get; init; }
    public IFileInfo FileInfo { get; init; }
}