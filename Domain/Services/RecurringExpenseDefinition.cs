namespace Domain.Services;

public enum RecurringFrequency
{
    Weekly, Biweekly, Monthly
}

public enum Way
{
    Fixed, PriorPeriod
}

public record RecurringExpenseDefinition(
    string Name,
    Category Category,
    SubCategory? SubCategory,
    Shop? Shop,
    RecurringFrequency Frequency,
    Way Way,
    Money? ExpectedAmount
);