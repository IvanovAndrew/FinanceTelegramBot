using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class SpendingHistoryDTO
{
    [DataMember]
    public string Currency { get; set; }

    [DataMember]
    public List<MonthSpendingDTO> Months { get; set; }
}

[DataContract]
public class SpendingDayHistoryDTO
{
    [DataMember]
    public string Currency { get; set; }

    [DataMember]
    public List<DaySpendingDTO> Days { get; set; }
}

[DataContract]
public class DaySpendingDTO
{
    [DataMember]
    public DateOnly Day { get; set; } 

    [DataMember]
    public decimal Total { get; set; }

    [DataMember]
    public List<ShopSpendingDTO> Shops { get; set; }
}

[DataContract]
public class MonthSpendingDTO
{
    [DataMember]
    public DateOnly Month { get; set; } 

    [DataMember]
    public decimal Total { get; set; }
    
    [DataMember]
    public decimal TotalOutcome { get; set; }

    [DataMember]
    public decimal RealOutcomeTotal { get; set; }
    
    [DataMember]
    public decimal TotalIncome { get; set; }

    [DataMember]
    public List<CategorySpendingDTO> OutcomeCategories { get; set; }
    
    [DataMember]
    public List<CategorySpendingDTO> IncomeCategories { get; set; }
}

[DataContract]
public class ShopSpendingDTO
{
    public string Name { get; set; }
    public List<CategorySpendingDTO> Categories { get; set; }
}

[DataContract]
public class CategorySpendingDTO
{
    [DataMember]
    public string Category { get; set; } 

    [DataMember]
    public decimal Total { get; set; }

    [DataMember]
    public List<SubCategorySpendingDTO> SubCategories { get; set; }
}

[DataContract]
public class SubCategorySpendingDTO
{
    [DataMember]
    public string SubCategory { get; set; }

    [DataMember]
    public decimal Total { get; set; }
}