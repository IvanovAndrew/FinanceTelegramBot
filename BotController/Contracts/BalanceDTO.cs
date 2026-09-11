using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class BalanceDTO 
{
    [DataMember]
    public decimal TotalIncome { get; set; }
    [DataMember]
    public decimal TotalOutcome { get; set; }
    [DataMember]
    public decimal TotalBalance { get; set; }
    [DataMember]
    public List<FutureExpenseDTO> FutureExpenses { get; set; }
    [DataMember]
    public decimal FutureExpensesTotal { get; set; }
    [DataMember]
    public decimal RealFreeMoney { get; set; }
    
    
    [DataMember]
    public DateTime StartPeriod { get; set; }
    [DataMember]
    public DateTime Payday { get; set; }
    [DataMember]
    public int DaysUntilPayday { get; set; }
    
    [DataMember]
    public decimal DailyBudgetLimit { get; set; }
    [DataMember]
    public string Currency { get; set; }
}

[DataContract]
public class FutureExpenseDTO
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public string? Shop { get; set; }
    
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}