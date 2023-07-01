﻿namespace Domain;

public interface IExpenseRepository
{
    Task Save(IExpense expense, CancellationToken cancellationToken);
    Task<List<IExpense>> Read(Predicate<DateOnly> dateFilter, CancellationToken cancellationToken);
}