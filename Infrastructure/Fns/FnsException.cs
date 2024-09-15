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