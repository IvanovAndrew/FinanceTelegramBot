namespace Domain.Services;

public interface ISalaryDayService
{
    DateOnly GetSalaryDay(DateOnly previousSalaryDay);
}