using Domain;
using Domain.Services;
using Infrastructure;
using Microsoft.Extensions.Logging;
using UnitTest;
using Xunit;

namespace Application.Test;

public class DateTimeServiceTest
{
    [Fact]
    public void When_The_First_Day_Is_Monday_Then_First_Working_Day_Is_The_First_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 11, 1));
        
        Assert.Equal(new DateOnly(2025, 12, 1), firstWorkingDay);
    }
    
    [Fact]
    public void When_The_First_Day_Is_Tuesday_Then_First_Working_Day_Is_The_First_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 6, 1));
        
        Assert.Equal(new DateOnly(2025, 7, 1), firstWorkingDay);
    }
    
    [Fact]
    public void When_The_First_Day_Is_Wednesday_Then_First_Working_Day_Is_The_First_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 9, 1));
        
        Assert.Equal(new DateOnly(2025, 10, 1), firstWorkingDay);
    }
    
    [Fact]
    public void When_The_First_Day_Is_Thursday_Then_First_Working_Day_Is_The_First_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 4, 1));
        
        Assert.Equal(new DateOnly(2025, 5, 1), firstWorkingDay);
    }
    
    [Fact]
    public void When_The_First_Day_Is_Friday_Then_First_Working_Day_Is_The_First_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 7, 1));
        
        Assert.Equal(new DateOnly(2025, 8, 1), firstWorkingDay);
    }
    
    [Fact]
    public void When_The_First_Day_Is_Saturday_Then_First_Working_Day_Is_The_Third_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 10, 1));
        
        Assert.Equal(new DateOnly(2025, 11, 3), firstWorkingDay);
    }
    
    [Fact]
    public void When_The_First_Day_Is_Sunday_Then_First_Working_Day_Is_The_Second_Day()
    {
        ISalaryDayService salaryDayService = CreateSalaryDayService();

        var firstWorkingDay = salaryDayService.GetSalaryDay(new DateOnly(2025, 5, 1));
        
        Assert.Equal(new DateOnly(2025, 6, 2), firstWorkingDay);
    }

    private ISalaryDayService CreateSalaryDayService()
    {
        return new SalaryDayService(new SalarySettings()
            { PaymentDay = 1, ShiftWhenHoliday = "next", Holidays = Array.Empty<DayWithoutYear>() }, new LoggerStub<SalaryDayService>());
    }
}