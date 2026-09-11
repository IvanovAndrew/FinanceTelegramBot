using Domain;

namespace Infrastructure.GoogleSpreadsheet;

public class GoogleSpreadsheetServiceException(string message, Exception innerException, bool isTransient = false)
    : FinanceRepositoryException(message, innerException, isTransient)
{
    
}