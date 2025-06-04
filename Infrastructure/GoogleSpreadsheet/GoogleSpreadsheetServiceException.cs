namespace Infrastructure.GoogleSpreadsheet;

public class GoogleSpreadsheetServiceException : Exception
{
    public GoogleSpreadsheetServiceException(string message) : base(message)
    {
        
    }
}

public class GoogleSpreadsheetServiceMissingParameterException : GoogleSpreadsheetServiceException
{
    public GoogleSpreadsheetServiceMissingParameterException(string parameter) : base($"Parameter {parameter} is missing")
    {
        
    }
}