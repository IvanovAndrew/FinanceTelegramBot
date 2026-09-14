using Domain;

namespace Application.Core;

public record Check
{
    public IReadOnlyCollection<Outcome> Outcomes { get; init; } = new List<Outcome>();
    public IReadOnlyList<NewOption> NewOptions { get; init; } = [];
}

public record NewOption(string Code, string Description);