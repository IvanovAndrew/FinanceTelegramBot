using Domain.Services;

namespace Application.Test.Stubs;

public class SalaryDayServiceStub : ISalaryDayService
{
    public DateOnly SalaryDay { get; init; }
    
    public DateOnly GetSalaryDay(DateOnly previousSalaryDay) => SalaryDay;
}