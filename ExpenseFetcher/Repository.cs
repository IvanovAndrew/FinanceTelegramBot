using Supabase;

namespace ExpenseFetcher;

public class Repository
{
    private readonly Client _supabase;
    private bool Initialized = false;
    public Repository(Client supabase)
    {
        _supabase = supabase;
    }

    private async ValueTask InitializeIfRequired()
    {
        if (Initialized)
            return;

        await _supabase.InitializeAsync();
    }

    public async Task<bool> SaveExpense(DbExpense expense)
    {
        await InitializeIfRequired();
        var response = await _supabase.From<DbExpense>().Insert(expense);

        return true;
    }
    
    public async Task<bool> SaveBatchExpenses(List<DbExpense> expenses)
    {
        await InitializeIfRequired();
        var response = await _supabase.From<DbExpense>().Insert(expenses);

        return true;
    }
    
    public async Task<bool> Delete(List<DbExpense> expenses)
    {
        await InitializeIfRequired();
        var response = await _supabase.From<DbExpense>().Delete(expenses);

        return true;
    }
    
    public async Task<List<DbCategory>> ReadCategory()
    {
        var response = await _supabase.From<DbCategory>().Get();
        return response.Models;
    }
    
    public async Task<List<DbSubCategory>> ReadSubcategory()
    {
        var response = await _supabase.From<DbSubCategory>().Get();
        return response.Models;
    }

    public async Task<List<DbCurrency>> ReadCurrency()
    {
        var response = await _supabase.From<DbCurrency>().Get();
        return response.Models;
    }
}