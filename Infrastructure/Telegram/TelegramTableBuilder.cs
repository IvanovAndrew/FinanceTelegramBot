using Domain;

namespace Infrastructure.Telegram;

public class TelegramTableBuilder
{
    private string[,] _table;
    private int _rows;
    private int _columns;
    private int _currentRow;
    
    public TelegramTableBuilder(int rows, int columns)
    {
        _rows = rows;
        _columns = columns;
        _currentRow = 0;
        _table = new string[rows, columns];
    }
    
    public void FillRow(string title, string value)
    {
        for (int i = 0; i < _columns; i++)
        {
            _table[_currentRow, i] = i == 0? title : value;
        }

        _currentRow++;
    }
        
    public void FillRow<T>(string title, ExpenseInfo<T> expenseInfo, Currency[] currencies)
    {
        for (int i = 0; i < _columns; i++)
        {
            if (i == 0)
            {
                _table[_currentRow, i] = title;
            }
            else
            {
                _table[_currentRow, i] = expenseInfo[currencies[i - 1]].ToString();
            }
        }
        _currentRow++;
    }

    public string[,] Build() => _table;
}