using System.Text;

namespace Infrastructure
{
    public static class MarkdownFormatter
    {
        public static string FormatTable(TableOptions tableOptions, string[,] rows)
        {
            if (tableOptions.FirstColumnName == null) throw new ArgumentException("Missing table title");
            if (rows == null || rows.Length == 0) throw new ArgumentException("Missing table values");

            int[] columnWidth = CalculateColumnWidth(tableOptions.AllColumns, rows);
        
            var builder = new StringBuilder();
            
            if (!string.IsNullOrEmpty(tableOptions.Title))
            {
                builder.AppendLine(tableOptions.Title);
            }
        
            for (int i = 0; i < tableOptions.AllColumns.Length; i++)
            {
                if (i != 0)
                {
                    builder.Append("|");
                }
                builder.Append($@"{tableOptions.AllColumns[i].PadLeft(columnWidth[i])}");
            }
            builder.AppendLine();
        
            for (int i = 0; i < tableOptions.AllColumns.Length; i++)
            {
                if (i != 0)
                {
                    builder.Append("|");
                }
                builder.Append(@$"{new string('-', columnWidth[i])}");
            }

            builder.AppendLine();

            int rowsCount = rows.GetLength(0);
            for (int i = 0; i < rowsCount; i++)
            {
                for(int j = 0; j < rows.GetLength(1); j++)
                {
                    builder.Append($"{rows[i, j]}".PadLeft(columnWidth[j]));
                    if (j != rows.GetLength(1) - 1)
                    {
                        builder.Append(@"|");
                    }
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static int[] CalculateColumnWidth(string[] titles, string[,] rows)
        {
            int[] widths = new int[rows.GetLength(1)];

            for (int i = 0; i < widths.Length; i++)
            {
                var titleWidth = titles[i].Length;

                int valuesMaxWidth = 0;
                for (int j = 0; j < rows.GetLength(0); j++)
                {
                    if (valuesMaxWidth < rows[j, i].Length)
                    {
                        valuesMaxWidth = rows[j, i].Length;
                    } 
                }

                widths[i] = titleWidth > valuesMaxWidth ? titleWidth : valuesMaxWidth;
            }

            return widths;
        }
    }
}