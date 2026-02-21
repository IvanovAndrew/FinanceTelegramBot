using Domain.Services;

namespace Application.Test.Stubs;

public class SalaryDayServiceStub : ISalaryDayService
{
    public DateOnly SalaryDay { get; set; }
    
    public DateOnly GetSalaryDay(DateOnly previousSalaryDay) => previousSalaryDay.AddMonths(1);
}