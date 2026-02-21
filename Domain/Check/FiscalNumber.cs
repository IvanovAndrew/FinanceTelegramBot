using System.Text.RegularExpressions;

namespace Domain.Check;

public record FiscalNumber
{
    private readonly string _fiscalNumber;

    private FiscalNumber(string fiscalNumber)
    {
        _fiscalNumber = fiscalNumber;
    }

    public static Result<FiscalNumber> Create(string text)
    {
        if (!Regex.IsMatch(text.Trim(), "\\d{16}"))
        {
            return Result<FiscalNumber>.Failure($"Fiscal number should contain 16 digits");
        }
        
        return Result<FiscalNumber>.Success(new FiscalNumber(text));
    }

    public override string ToString() => _fiscalNumber;
}

public partial record FiscalDocumentNumber
{
    private readonly string _fiscalDocumentNumber;

    private FiscalDocumentNumber(string fiscalDocumentNumber)
    {
        _fiscalDocumentNumber = fiscalDocumentNumber;
    }

    public static Result<FiscalDocumentNumber> Create(string text)
    {
        if (!DigitsOnlyRegex().IsMatch(text))
        {
            return Result<FiscalDocumentNumber>.Failure($"Fiscal document number should contain only digits");
        }
        
        return Result<FiscalDocumentNumber>.Success(new FiscalDocumentNumber(text));
    }

    public override string ToString() => _fiscalDocumentNumber;
    
    [GeneratedRegex(@"^\d+$")]
    private static partial Regex DigitsOnlyRegex();
}

public partial record FiscalDocumentSign
{
    private readonly string _fiscalDocumentSign;

    private FiscalDocumentSign(string fiscalDocumentSign)
    {
        _fiscalDocumentSign = fiscalDocumentSign;
    }

    public static Result<FiscalDocumentSign> Create(string text)
    {
        if (!DigitsOnlyRegex().IsMatch(text))
        {
            return Result<FiscalDocumentSign>.Failure($"Fiscal document sign should contain only digits");
        }
        
        return Result<FiscalDocumentSign>.Success(new FiscalDocumentSign(text));
    }

    public override string ToString() => _fiscalDocumentSign;
    
    [GeneratedRegex(@"^\d+$")]
    private static partial Regex DigitsOnlyRegex();
}