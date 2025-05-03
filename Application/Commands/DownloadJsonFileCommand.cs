using MediatR;

namespace Application.Commands;

public class DownloadJsonFileCommand : IRequest
{
    public long SessionId { get; init; }
    public IFileInfo FileInfo { get; init; }
}