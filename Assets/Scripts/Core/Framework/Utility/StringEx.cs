using System;
using System.Linq;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        switch (input)
        {
            case null: throw new ArgumentNullException(nameof(input));
            case "": return input;
            default: return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}