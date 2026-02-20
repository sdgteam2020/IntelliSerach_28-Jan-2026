using System.Text.RegularExpressions;

namespace AIDocSearch.CustomMiddleware
{
    public static class XssHelper
    {
        private static readonly Regex ScriptRegex =
            new Regex(@"<\s*script|javascript:|vbscript:|onerror\s*=|onload\s*=",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool ContainsXss(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Simple quick checks
            if (input.Contains("<") || input.Contains(">"))
                return true;

            // More specific patterns
            if (ScriptRegex.IsMatch(input))
                return true;

            return false;
        }
    }
}