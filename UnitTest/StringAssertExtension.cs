using NUnit.Framework;

namespace UnitTest;

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

            Assert.That(prevIndex < currentIndex, message);
        }
    }
}