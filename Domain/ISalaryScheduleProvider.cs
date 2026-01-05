namespace Domain;

public interface ISalaryScheduleProvider
{
    SalarySchedule GetFrom(IEnumerable<Income> incomes);
}

public class SalarySchedule
{
    public DateOnly SalaryDay { get; init; }
}

public class SalaryScheduleProvider : ISalaryScheduleProvider
{
    public SalarySchedule GetFrom(IEnumerable<Income> incomes)
    {
        var salaryIncome = incomes.Where(i => i.IsSalary());

        if (!salaryIncome.Any())
            return new SalarySchedule(){SalaryDay = new DateOnly(2000, 1, 1)};

        return new SalarySchedule(){SalaryDay = salaryIncome.Max(i => i.Date)};
    }
}

public class SalaryNotFoundException : Exception
{
}
