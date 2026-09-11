using Application.Bot;

namespace Application.Core;

public class FlowInputValidationException(FlowStep step, string resultError) : Exception(resultError)
{
    public FlowStep Step { get; } = step;
}