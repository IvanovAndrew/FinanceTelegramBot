namespace Domain;

public sealed class CategoryStatisticQuery(
    DateOnly? dateFrom,
    Category? category,
    Currency? currency)
{
    public DateOnly? DateFrom { get; } = dateFrom;
    public Category? Category { get; } = category;
    public Currency? Currency { get; } = currency;
}