using Domain;

namespace Application;

public class UserSession
{
    public long Id { get; init; }
    public int? LastSentMessageId { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public MoneyTransferBuilder? MoneyTransferBuilder { get; set; }
    public StatisticsOptions StatisticsOptions { get; set; }
    public CheckRequisite? CheckRequisites { get; set; }
    public IQuestionnaireService? QuestionnaireService { get; set; }
}

public enum SessionState
{
    None,             // Нет активного действия
    ChoosingOutcome,  // Выбор способа внесения расхода
    OutcomeByMyself,  // Внесение расхода вручную
    OutcomeFromCheck, // Выбор метода загрузки чека
    OutcomeUrlFromQr, // Внесение расхода через QR
    OutcomeJson,      // Внесение расхода через JSON
    OutcomeRequisites,// Внесение расхода по реквизитам
    ChoosingIncome,   // Внесение дохода
    ChoosingStatistics, // Выбор статистики
    StatisticsBalance,
    StatisticsDay,
    StatisticsMonth,
    StatisticsCategory,
    StatisticsSubcategory,
    StatisticsSubcategoryMonth
}

public class StatisticsOptions
{
    public Category? Category { get; set; }
    public SubCategory? Subcategory { get; set; }
    public bool CurrencySpecified { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Currency? Currency { get; set; }
}

// public class MoneyTransferUserSession : UserSession
// {
//     public MoneyTransferBuilder MoneyTransferBuilder { get; internal set; }
//     public override SessionState GetSessionState()
//     {
//         if (MoneyTransferBuilder.Date == null)
//         {
//             return SessionState.MoneyTransferDateRequested;
//         }
//
//         if (MoneyTransferBuilder.Category == null)
//         {
//             return SessionState.MoneyTransferCategoryRequested;
//         }
//
//         if (MoneyTransferBuilder.Category != null && MoneyTransferBuilder.Category.Subcategories.Any() &&
//             MoneyTransferBuilder.SubCategory == null)
//         {
//             return SessionState.MoneyTranferSubcategoryRequested;
//         }
//
//         if (MoneyTransferBuilder.Category != null && !MoneyTransferBuilder.Category.Subcategories.Any() &&
//             MoneyTransferBuilder.Description == null)
//         {
//             return SessionState.MoneyTranferDescriptionRequested;
//         }
//         
//         if (MoneyTransferBuilder.Sum == null)
//         {
//             return SessionState.MoneyTranferPriceRequested;
//         }
//
//         return SessionState.MoneyTranferSaveRequested;
//     }
// }
//
// public abstract class StatisticUserSession : UserSession
// {
//     public abstract FinanceFilter CreateFinanceFilter();
// }
//
// public class StatisticBySubcategoryByMonthUserSession : StatisticUserSession
// {
//     public DateOnly? StartMonth { get; set; }
//     public Category? Category { get; set; }
//     public Currency? Currency { get; set; }
//     public bool CurrencySpecified { get; set; }
//     public override SessionState GetSessionState()
//     {
//         if (StartMonth == null)
//         {
//             return SessionState.StatisticBySubcategoryByMonthDateRequested;
//         }
//
//         if (Category != null)
//         {
//             return SessionState.StatisticBySubcategoryByMonthCategoryRequested;
//         }
//
//         if (!CurrencySpecified)
//         {
//             return SessionState.StatisticBySubcategoryByMonthCurrencyRequested;
//         }
//
//         return SessionState.StatisticRequested;
//     }
//
//     public override FinanceFilter CreateFinanceFilter()
//     {
//         return new FinanceFilter()
//         {
//             DateFrom = StartMonth,
//             Category = Category.Name,
//             Currency = Currency
//         };
//     }
// }
//
// public class StatisticByCategoryUserSession : StatisticUserSession
// {
//     public DateOnly? StartMonth { get; set; }
//     public Currency? Currency { get; set; }
//     public bool CurrencySpecified { get; set; }
//     public override SessionState GetSessionState()
//     {
//         if (StartMonth == null)
//         {
//             return SessionState.StatisticByCategoryDateRequested;
//         }
//
//         if (!CurrencySpecified)
//         {
//             return SessionState.StatisticByCategoryCurrencyRequested;
//         }
//
//         return SessionState.StatisticRequested;
//     }
//
//     public override FinanceFilter CreateFinanceFilter()
//     {
//         return new FinanceFilter()
//         {
//             DateFrom = StartMonth,
//             Currency = Currency,
//         };
//     }
// }
//
// public class StatisticBySubcategoryUserSession : StatisticUserSession
// {
//     public DateOnly? StartMonth { get; set; }
//     public Currency? Currency { get; set; }
//     public Category? Category { get; set; }
//     public SubCategory? Subcategory { get; set; }
//     public bool CurrencySpecified { get; set; }
//     public override SessionState GetSessionState()
//     {
//         if (Category == null)
//         {
//             return SessionState.StatisticBySubcategoryCategoryRequested;
//         }
//         
//         if (Subcategory == null)
//         {
//             return SessionState.StatisticBySubcategorySubcategoryRequested;
//         }
//         
//         if (StartMonth == null)
//         {
//             return SessionState.StatisticBySubcategoryDateRequested;
//         }
//
//         if (!CurrencySpecified)
//         {
//             return SessionState.StatisticBySubcategoryCurrencyRequested;
//         }
//
//         return SessionState.StatisticRequested;
//     }
//
//     public override FinanceFilter CreateFinanceFilter()
//     {
//         return new FinanceFilter()
//         {
//             DateFrom = StartMonth,
//             Currency = Currency,
//         };
//     }
// }
//
// public class StatisticByDayUserSession : StatisticUserSession
// {
//     public DateOnly? Date { get; set; }
//     public override SessionState GetSessionState()
//     {
//         if (Date == null)
//         {
//             return SessionState.StatisticByDayDateRequested;
//         }
//
//         return SessionState.StatisticRequested;
//     }
//
//     public override FinanceFilter CreateFinanceFilter()
//     {
//         return new FinanceFilter()
//         {
//             DateFrom = Date,
//             DateTo = Date,
//         };
//     }
// }
//
// public class StatisticByMonthUserSession : StatisticUserSession
// {
//     public DateOnly? Date { get; set; }
//     public Currency? Currency { get; set; }
//     public bool CurrencySpecified { get; set; }
//     
//     public override SessionState GetSessionState()
//     {
//         if (Date == null)
//         {
//             return SessionState.StatisticByMonthMonthRequested;
//         }
//
//         if (!CurrencySpecified)
//         {
//             return SessionState.StatisticByMonthCurrencyRequested;
//         }
//
//         return SessionState.StatisticByMonthRequested;
//     }
//
//     public override FinanceFilter CreateFinanceFilter()
//     {
//         return new FinanceFilter()
//         {
//             DateFrom = Date,
//             DateTo = Date,
//             Currency = Currency
//         };
//     }
// }
//
// public enum SessionState
// {
//     StatisticByDayDateRequested,
//     
//     StatisticByMonthMonthRequested,
//     StatisticByMonthCurrencyRequested,
//     StatisticByMonthRequested,
//     
//     
//     StatisticByCategoryDateRequested,
//     StatisticByCategoryCurrencyRequested,
//     
//     StatisticBySubcategoryDateRequested,
//     StatisticBySubcategoryCategoryRequested,
//     StatisticBySubcategorySubcategoryRequested,
//     StatisticBySubcategoryCurrencyRequested,
//     
//     StatisticBySubcategoryByMonthDateRequested,
//     StatisticBySubcategoryByMonthCategoryRequested,
//     StatisticBySubcategoryByMonthCurrencyRequested,
//     
//     MoneyTransferDateRequested,
//     MoneyTransferCategoryRequested,
//     MoneyTranferSubcategoryRequested,
//     MoneyTranferDescriptionRequested,
//     MoneyTranferPriceRequested,
//     MoneyTranferSaveRequested,
//     
//     StatisticRequested,
//     
// }
//
// public class InitialSession : UserSession
// {
//     public override SessionState GetSessionState()
//     {
//         return SessionState.MoneyTranferSaveRequested;
//     }
// }

public class CheckRequisites
{
    public DateTime? Date { get; set; }
    public decimal? Price { get; set; }
    public string? FiscalNumber { get; set; }
    public string? FiscalDocumentNumber { get; set; }
    public string? FiscalDocumentSign { get; set; }
}