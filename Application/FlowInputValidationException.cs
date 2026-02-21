namespace Application;

public class FlowInputValidationException(FlowStep step, string resultError) : Exception(resultError)
{
    public FlowStep Step { get; } = step;
}