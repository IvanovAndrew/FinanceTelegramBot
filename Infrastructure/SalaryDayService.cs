using Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public record DayWithoutYear(int Month, int Day);

public class SalarySettings
{
    public int PaymentDay { get; set; }
    public string ShiftWhenHoliday { get; set; }
    public DayWithoutYear[] Holidays { get; set; }
}


public class SalaryDayService(SalarySettings settings, ILogger<SalaryDayService> logger) : ISalaryDayService
{
    public SalaryDayService(IOptions<SalarySettings> options, ILogger<SalaryDayService> logger) : this(options.Value, logger)
    {
    }

    public DateOnly GetSalaryDay(DateOnly previousSalaryDay)
    {
        previousSalaryDay.AddMonths(1).Deconstruct(out var year, out var month, out var day);
        
        var salaryDay = new DateOnly(year, month, settings.PaymentDay);

        while (!IsWorkingDay(salaryDay))
        {
            salaryDay = salaryDay.AddDays(settings.ShiftWhenHoliday == "next" ? 1 : -1);
        }

        return salaryDay;
    }

    private bool IsWorkingDay(DateOnly day)
    {
        return !IsWeekend(day) && !IsHoliday(day);
        
        bool IsWeekend(DateOnly d) => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        bool IsHoliday(DateOnly d) => settings.Holidays.Any(holiday => holiday.Day == d.Day && holiday.Month == d.Month);
    }
}