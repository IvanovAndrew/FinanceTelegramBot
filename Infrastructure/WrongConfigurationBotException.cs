namespace StateMachine;

public class WrongConfigurationBotException : BotException
{
    public WrongConfigurationBotException(string parameter) : base($"{parameter} isn't configured properly")
    {
    }
}