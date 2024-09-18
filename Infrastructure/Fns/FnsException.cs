namespace Infrastructure.Fns;

public class FnsException : Exception
{
    private string _message;
    public override string Message => $"FNS Error: {_message}"; 
    public FnsException(string message) : base(message)
    {
        _message = message;
    }
}

public class WrongConfigurationFnsException : FnsException
{
    public WrongConfigurationFnsException(string parameter) : base($"Parameter {parameter} isn't configured properly")
    {
    }
}