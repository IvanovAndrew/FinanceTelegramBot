﻿namespace Domain;

public class ExpenseFromDateRangeSpecification : ISpecification<IExpense>
{
    private readonly DateOnly _from;
    private readonly DateOnly _to;
    public ExpenseFromDateRangeSpecification(DateOnly from, DateOnly to)
    {
        _from = from;
        _to = to;
    }
    
    public bool IsSatisfied(IExpense item)
    {
        return _from <= item.Date && item.Date <= _to;
    }
}