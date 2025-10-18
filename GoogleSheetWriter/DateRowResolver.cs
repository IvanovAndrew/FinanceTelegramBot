namespace GoogleSheetWriter;

public class DateRowResolver(Dictionary<DateOnly, int> map)
{
    public int GetBestFirstRow(DateOnly date, int defaultValue = 1)
    {
        var candidates = map.Keys.Where(d => d <= date);

        if (!candidates.Any())
            return defaultValue;

        return map[candidates.Max()];
    }
}