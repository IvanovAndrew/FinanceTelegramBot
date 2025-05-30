namespace Infrastructure;

public class BotException : Exception
{
    public override string Message { get; }

    public BotException(string message) : base(message)
    {
        Message = message;
    }
}

public class BotStateException : BotException
{
    public BotStateException(string[] expected, string actual) : base($"Expected values are {string.Join(", ", expected)}. {actual} was received.")
    {
        
    }
}

public class MissingInformationBotException : BotException
{
    public MissingInformationBotException() : base("I've forgotten the previous messages. Please enter /start and add your expense again")
    {
        
    }
}