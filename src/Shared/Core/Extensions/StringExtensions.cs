using System.Globalization;

namespace Jamesnet.Foundation;

public static class StringExtensions
{
    private static readonly TextInfo EnglishText = new CultureInfo("en-US", false).TextInfo;

    /// <summary>"hello world" → "HelloWorld". 현재 문화권(예: 터키어 i)과 무관하게 같은 결과를 낸다.</summary>
    public static string ToPascal(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return EnglishText.ToTitleCase(EnglishText.ToLower(input)).Replace(" ", "");
    }
}
