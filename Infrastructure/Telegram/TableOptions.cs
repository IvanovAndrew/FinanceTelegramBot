namespace Infrastructure.Telegram;

public class TableOptions
{
    public string Title { get; init; }
    public string FirstColumnName { get; init; }
    public string[] OtherColumns { get; set; }

    public string[] AllColumns
    {
        get
        {
            string[] columns = new string[OtherColumns.Length + 1];
            columns[0] = FirstColumnName;

            for (int i = 0; i < OtherColumns.Length; i++)
            {
                columns[i + 1] = OtherColumns[i];
            }

            return columns;
        }
    }
}