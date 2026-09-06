using System.Reflection;
using Google.Apis.Util;

namespace GoogleSheetWriter
{
    public class ListInfo
    {
        public bool IsIncome;
        
        public string ListName;
        public string? Category;

        [ExcelColumn(Write = true, Read = false)]
        public ExcelColumn? YearColumn;
        
        [ExcelColumn(Write = true, Read = false)]
        public ExcelColumn? MonthColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn DateColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? CategoryColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? SubCategoryColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? ShopColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? DescriptionColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn AmountRurColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn AmountAmdColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn OtherAmountColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? OtherCurrencyColumn;
        
        public DateRowResolver? DateRowResolver;
        
        public ExcelColumn GetLastExcelColumn()
        {
            var columnValues = this.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Utilities.GetCustomAttribute<ExcelColumnAttribute>(p) != null)
                .Select(p => p.GetValue(this) as ExcelColumn)
                .Where(s => s != null);

            return columnValues.Max();
        }
        
        public ExcelColumn GetFirstExcelColumn()
        {
            var columnValues = this.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Utilities.GetCustomAttribute<ExcelColumnAttribute>(p) != null)
                .Select(p => p.GetValue(this) as ExcelColumn)
                .Where(s => s != null);

            return columnValues.Min();
        }

        

        public override string ToString()
        {
            return string.Join($", {Environment.NewLine}",
                $"Category = {Category}",
                $"YearColumn = {YearColumn}",
                $"MonthColumn = {MonthColumn}",
                $"DateColumn = {DateColumn}",
                $"CategoryColumn = {CategoryColumn}",
                $"SubCategoryColumn = {SubCategoryColumn}",
                $"DescriptionColumn = {DescriptionColumn}",
                $"AmountRurColumn = {AmountRurColumn}",
                $"AmountAmdColumn = {AmountAmdColumn}",
                $"{nameof(OtherAmountColumn)} = {OtherAmountColumn}",
                $"{nameof(OtherCurrencyColumn)} = {OtherCurrencyColumn}");
        }
    }

    public class CurrencyExchangeListInfo
    {
        public string ListName;
        public string? Category;

        [ExcelColumn(Write = true, Read = false)]
        public ExcelColumn? YearColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn DateColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? ShopColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? DescriptionColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn SourceAmountColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn SourceCurrencyColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn TargetAmountColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? TargetCurrencyColumn;
        
        public DateRowResolver? DateRowResolver;
        
        public ExcelColumn GetLastExcelColumn()
        {
            var columnValues = this.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Utilities.GetCustomAttribute<ExcelColumnAttribute>(p) != null)
                .Select(p => p.GetValue(this) as ExcelColumn)
                .Where(s => s != null);

            return columnValues.Max();
        }
        
        public ExcelColumn GetFirstExcelColumn()
        {
            var columnValues = this.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Utilities.GetCustomAttribute<ExcelColumnAttribute>(p) != null)
                .Select(p => p.GetValue(this) as ExcelColumn)
                .Where(s => s != null);

            return columnValues.Min();
        }

        

        public override string ToString()
        {
            return string.Join($", {Environment.NewLine}",
                $"{nameof(YearColumn)} = {YearColumn}",
                $"{nameof(DateColumn)} = {DateColumn}",
                $"{nameof(DescriptionColumn)} = {DescriptionColumn}",
                $"{nameof(SourceAmountColumn)} = {SourceAmountColumn}",
                $"{nameof(SourceCurrencyColumn)} = {SourceCurrencyColumn}",
                $"{nameof(TargetAmountColumn)} = {TargetAmountColumn}",
                $"{nameof(TargetCurrencyColumn)} = {TargetCurrencyColumn}");
        }
    }

    public class FutureExpenseListInfo
    {
        public string ListName;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn NameColumn;

        [ExcelColumn(Write = false, Read = false)]
        public ExcelColumn? CategoryColumn;
        
        [ExcelColumn(Write = false, Read = true)]
        public ExcelColumn SubCategoryColumn;
        
        [ExcelColumn(Write = false, Read = true)]
        public ExcelColumn ShopColumn;
        
        [ExcelColumn(Write = false, Read = true)]
        public ExcelColumn FrequencyColumn;
        
        [ExcelColumn(Write = false, Read = true)]
        public ExcelColumn WayColumn;
        
        [ExcelColumn(Write = false, Read = true)]
        public ExcelColumn SumColumn;
        
        [ExcelColumn(Write = false, Read = true)]
        public ExcelColumn CurrencyColumn;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ExcelColumnAttribute : Attribute
    {
        public bool Read { get; set; } = true;
        public bool Write { get; set; } = true;
    }
}