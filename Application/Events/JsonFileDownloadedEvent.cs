using MediatR;

namespace Application.Events;

public class JsonFileDownloadedEvent : INotification
{
    public long SessionId { get; init; }
    public string Json { get; init; }
}