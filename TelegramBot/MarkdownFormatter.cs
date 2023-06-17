using System;
using System.Text;

namespace TelegramBot
{
    public class TableOptions
    {
        public string Title { get; init; }
        public string[] ColumnNames { get; init; }
    }
    
    public static class MarkdownFormatter
    {
        public static string FormatTable(TableOptions tableOptions, string[,] rows)
        {
            if (tableOptions.ColumnNames == null || tableOptions.ColumnNames.Length == 0) throw new ArgumentException("Missing table titles");
            if (rows == null || rows.Length == 0) throw new ArgumentException("Missing table values");

            int[] columnWidth = CalculateColumnWidth(tableOptions.ColumnNames, rows);
        
            var builder = new StringBuilder();

            // telegram unfixed bug: leading symbol is trimmed
            // https://github.com/telegramdesktop/tdesktop/issues/1521
            builder.AppendLine(@"|");
            
            if (!string.IsNullOrEmpty(tableOptions.Title))
            {
                builder.AppendLine(tableOptions.Title);
            }
        
            for (int i = 0; i < tableOptions.ColumnNames.Length; i++)
            {
                builder.Append($@"|{tableOptions.ColumnNames[i].PadLeft(columnWidth[i])}");
            }
            builder.Append(@"|");
            builder.AppendLine();
        
            for (int i = 0; i < tableOptions.ColumnNames.Length; i++)
            {
                builder.Append(@$"|{new string('-', columnWidth[i])}");
            }

            builder.AppendLine();

            int rowsCount = rows.GetLength(0);
            for (int i = 0; i < rowsCount; i++)
            {
                builder.Append(@"|");
                for(int j = 0; j < rows.GetLength(1); j++)
                {
                    builder.Append($"{rows[i, j]}".PadLeft(columnWidth[j]));
                    builder.Append(@"|");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static int[] CalculateColumnWidth(string[] titles, string[,] rows)
        {
            int[] widths = new int[titles.Length];

            for (int i = 0; i < widths.Length; i++)
            {
                var titleWidth = titles[i].Length;

                int valuesMaxWidth = 0;
                for (int j = 0; j < rows.GetLength(0); j++)
                {
                    if (valuesMaxWidth < rows[j, i].Length) valuesMaxWidth = rows[j, i].Length; 
                }

                widths[i] = titleWidth > valuesMaxWidth ? titleWidth : valuesMaxWidth;
            }

            return widths;
        }
    }
}