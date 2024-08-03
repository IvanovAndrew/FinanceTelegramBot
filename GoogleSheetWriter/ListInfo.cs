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
    }
}