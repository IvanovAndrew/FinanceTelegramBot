using Application.Test.Stubs;

namespace Application.Test;

public sealed class BotTestEnvironment
{
    public required FinanceRepositoryStub Repo { get; init; }
    public required DateTimeServiceStub Time { get; init; }
    public required MessageServiceMock MessageService { get; init; }

    public required SalaryDayServiceStub SalaryDayService { get; init; }
    public required FnsReceiptProviderStub FnsReceiptProvider { get; init; }
    public required YerevanCityReceiptProviderStub YerevanCityReceiptProvider { get; init; }
    public required ExpenseJsonParserStub ExpenseJsonParser { get; init; }
}