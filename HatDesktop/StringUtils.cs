using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HatDesktop
{
    public static class StringUtils
    {
        public static string ReplaceQuotes(this string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }
            var array = s.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == '\"')
                {
                    array[i] = i == 0 || array[i - 1] == ' ' ? '«' : '»';
                }
            }
            return new string(array);
        }

        public static string ReplaceFirstCharToUpper(this string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }
            var letter = s[0];
            letter = Char.ToUpperInvariant(letter);
            var array = s.ToCharArray();
            array[0] = letter;
            return new string(array);
        }

        public static string ReplaceHyphenWithDash(this string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }

            var array = s.ToCharArray();
            for (int i = 1; i < array.Length - 1; i++)
            {
                if (array[i] == '-' && array[i - 1] == ' ' && array[i + 1] == ' ')
                {
                    array[i] = '—';
                }
            }

            return new string(array);
        }

        public static string RemoveSquareBrackets(this string s)
        {
            if (String.IsNullOrEmpty(s) || s.Length < 4)
            {
                return s;
            }

            return Regex.Replace(s, @"\[[^\]]+\]\s*", "");
        }

        public static string RemoveMultipleSpaces(this string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }

            var regex = new Regex("[ ]{2,}", RegexOptions.None);
            return regex.Replace(s, " ");
        }

        public static string AddDot(this string s)
        {
            var last = s.Last();
            if (last != '.' || last != '.')
            {
                return s + ".";
            }
            return s;
        }

        public static string FormatDescription(string description)
            =>
                description.Trim()
                    .ReplaceFirstCharToUpper()
                    .ReplaceHyphenWithDash()
                    .RemoveSquareBrackets()
                    .ReplaceQuotes()
                    .RemoveMultipleSpaces()
                    .Trim()
                    .AddDot();

        public static string FormatPhrase(string newPhrase) => newPhrase.Trim().ToLowerInvariant();
    }
}