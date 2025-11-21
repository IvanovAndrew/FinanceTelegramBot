using MediatR;

namespace Application.Commands;

public record DownloadJsonFileCommand : IRequest
{
    public long SessionId { get; init; }
    public IFileInfo FileInfo { get; init; }
}