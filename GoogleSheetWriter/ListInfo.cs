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
        public ExcelColumn? DescriptionColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn AmountRurColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn AmountAmdColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn AmountGelColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? AmountUsdColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? AmountEurColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? AmountTryColumn;
        
        [ExcelColumn(Write = true, Read = true)]
        public ExcelColumn? AmountRsdColumn;

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
                $"AmountGelColumn = {AmountGelColumn}",
                $"AmountUsdColumn = {AmountUsdColumn}");
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ExcelColumnAttribute : Attribute
    {
        public bool Read { get; set; } = true;
        public bool Write { get; set; } = true;
    }
}