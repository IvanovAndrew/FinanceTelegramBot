using Domain;

namespace Application.Core;

public record Check
{
    public IReadOnlyCollection<Outcome> Outcomes { get; init; } = new List<Outcome>();
    public HashSet<string> NewOptions { get; init; } = new HashSet<string>();
}