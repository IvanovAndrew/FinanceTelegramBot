namespace GoogleSheetWriter
{
    public class ListInfo
    {
        public string ListName;
        public string? Category;
        

        public string YearColumn;
        public string MonthColumn;
        public string DateColumn;
        public string CategoryColumn;
        public string SubCategoryColumn;
        public string DescriptionColumn;
        public string AmountRurColumn;
        public string AmountAmdColumn;
        public string AmountGelColumn;

        public Dictionary<int, int> YearToFirstExcelRow = new();

        public override string ToString()
        {
            return string.Join($", {System.Environment.NewLine}",
                $"Category = {Category}",
                $"YearColumn = {YearColumn}",
                $"MonthColumn = {MonthColumn}",
                $"DateColumn = {DateColumn}",
                $"CategoryColumn = {CategoryColumn}",
                $"SubCategoryColumn = {SubCategoryColumn}",
                $"DescriptionColumn = {DescriptionColumn}",
                $"AmountRurColumn = {AmountRurColumn}",
                $"AmountAmdColumn = {AmountAmdColumn}",
                $"AmountGelColumn = {AmountGelColumn}");
        }
    }
}