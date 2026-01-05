using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure;

public static class MemoryCacheExtensions
{
    public static IEnumerable<string> GetKeys(this MemoryCache cache)
    {
        var entries = cache.GetType()
            .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(cache) as dynamic;

        if (entries == null) yield break;

        foreach (var entry in entries)
        {
            object key = entry.GetType().GetProperty("Key").GetValue(entry, null);
            yield return (string) key;
        }
    }
}