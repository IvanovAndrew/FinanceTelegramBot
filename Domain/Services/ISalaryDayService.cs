namespace Domain.Services;

public interface ISalaryDayService
{
    DateOnly GetSalaryDay(DateOnly previousSalaryDay);
    int GetRemainingDays(DateOnly today, DateOnly salaryDay, bool includeToday) => salaryDay.DayNumber - today.DayNumber + (includeToday ? 1 : 0);
}