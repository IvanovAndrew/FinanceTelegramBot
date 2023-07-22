namespace StateMachine;

public class BotStateException : Exception
{
    public BotStateException(string[] expected, string actual) : base($"Expected values are {(string.Join(", ", expected))}. {actual} was received.")
    {
        
    }
}