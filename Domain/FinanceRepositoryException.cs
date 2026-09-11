namespace Domain;

public class FinanceRepositoryException : Exception
{
    public bool IsTransient { get; }

    public FinanceRepositoryException(string message, Exception? innerException = null, bool isTransient = false)
        : base(message, innerException)
    {
        IsTransient = isTransient;
    }
}