using Xunit;

namespace UnitTest.Extensions;

public static class StringAssertExtension
{
    internal static void AssertOrder(string text, params string[] substrings)
    {
        var prevIndex = -1;
        var currentIndex = -1;
        
        for (int i = 0; i < substrings.Length; i++)
        {
            prevIndex = currentIndex;
            currentIndex = text.IndexOf(substrings[i], StringComparison.InvariantCultureIgnoreCase);

            string message;
            if (i == 0)
            {
                message = $"{substrings[i]} is missing";
            }
            else
            {
                message = $"{substrings[i - 1]} should be before {substrings[i]}.";
            }

            Assert.True(prevIndex < currentIndex, message);
        }
    }
}

public static class CollectionAssertExtension
{
    internal static void AssertOrder(List<string> text, params string[] items)
    {
        var indices = new List<int>();
        foreach (var item in items)
        {
            indices.Add(text.IndexOf(item));
        }

        
        for (int i = 0; i < indices.Count; i++)
        {
            int previousValue = i > 0? indices[i - 1] : -1;
            string message;
            if (i == 0)
            {
                message = $"{items[i]} is missing";
            }
            else
            {
                message = $"{items[i - 1]} should be before {items[i]}.";
            }

            Assert.True(previousValue < indices[i], message);
        }
    }
}