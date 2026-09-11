namespace Application.Contracts;

public class ExternalApiException : Exception
{
    protected ExternalApiException(string s) : base(s)
    {
        
    }
}

public class YerevanCityAPIException : ExternalApiException
{
    protected YerevanCityAPIException(string s) : base(s)
    {
    }
}

public class InternalExternalApiException : YerevanCityAPIException
{
    public InternalExternalApiException(Exception e) : base(e.Message)
    {
        
    }
    
}

public class EmptyResponseException : YerevanCityAPIException
{
    public EmptyResponseException(string source) : base($"Server {source} returned an empty response")
    {
        
    }
}