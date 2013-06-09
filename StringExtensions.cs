/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Common.Helpers {
	/// <summary>
	/// Provides utility extension methods for System.string
	/// </summary>
	public static class StringExtensions {
		public static readonly Char[] MagicRegexChars = new[] { '^', '$', '.', '(', ')', '{', '\\', '[', '?', '+', '*', '|' };
		public static readonly Char[] MagicSqlLikeChars = new[] { '%', '_', '[', ']' };
		public static readonly Char[] AngleBrackets = new[] { '<', '>' };
        public static readonly Regex EmailPattern = new Regex(@"^[\w._%+-]+@[\w.-]+\.[A-Za-z]{2,4}$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public static bool IsValidEmailAddress(this string s) {
		    ArgumentValidator.ThrowIfNull(s, "s");
            return EmailPattern.IsMatch(s);
		}
        
        public static bool HasMagicRegexChars(string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return -1 != s.IndexOfAny(MagicRegexChars);
		}

		public static char LastChar(string s) {
			if (string.IsNullOrEmpty(s)) {
				throw (new InvalidOperationException("s must not be null or empty"));
			}

			return s[s.Length - 1];
		}

		/// <summary>
		/// Performs a <see cref="StringComparison.OrdinalIgnoreCase"/> comparison between two <see cref="String.Trim()">trimmed</see> strings.
		/// </summary>
		/// <returns>True if trimmed strings match in an ordinal case insensive comparison, otherwise false.</returns>
		public static bool NormalEquals(this string s, string stringToCompare) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return null != stringToCompare && s.Trim().Equals(stringToCompare.Trim(), StringComparison.OrdinalIgnoreCase);
		}
		
		public static bool OicEquals(this string s1, string s2) {
			ArgumentValidator.ThrowIfNull(s1, "s1");
			
			return null != s2 && s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
		}

		public static string Or(this string s, string alternative) {
			return string.IsNullOrEmpty(s) ? alternative : s;
		}

		public static string OrEmpty(this string s) {
			return s ?? string.Empty;
		}

		public static string Reverse(string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			var chars = s.ToCharArray();
			Array.Reverse(chars);

			return new string(chars);
		}

		/// <summary>
		/// Formats a string and arguments using the <see cref="CultureInfo.InvariantCulture">invariant culture</see>.
		/// </summary>
		/// <remarks>
		/// <para>This should <b>not</b> be used for any strings that are displayed to the user. It is meant for log
		/// messages, exception messages, and other types of information that do not make it into the UI, or wouldn't
		/// make sense to users anyway ;).</para>
		/// </remarks>
		public static string FormatInvariant(this string format, params object[] args) {
			ArgumentValidator.ThrowIfNull(format, "format");

			return 0 == args.Length ? format : string.Format(CultureInfo.InvariantCulture, format, args);
		}

		public static string FormatSql(this string format, params object[] args) {
			ArgumentValidator.ThrowIfNull(format, "format");

			return string.Format(SqlFormatter.Instance, format, args);
		}

        public static string FormatSqlTopStatement(this string format, int numberOfRecords)
        {
            return Database.DefaultProvider == DatabaseProviderType.MySql
                       ? MySqlTopStatement(format, numberOfRecords)
                       : SqlServerTopStatement(format, numberOfRecords);
        }

        public static string FormatSqlColumnName(this string format)
        {
            return Database.DefaultProvider == DatabaseProviderType.MySql
                       ? "`{0}`".Fi(format)
                       : "[{0}]".Fi(format);
        }

        private static string SqlServerTopStatement(string format, int numberOfRecords)
        {
            var statement = new SqlServerStatement();
            var top = statement.GetTopStatement(numberOfRecords);
            return Regex.Replace(format, @"\bSELECT\b", "SELECT {0}".Fi(top), RegexOptions.IgnoreCase);
        }

        private static string MySqlTopStatement(string format, int numberOfRecords)
        {
            var statement = new MySqlStatement();
            var top = statement.GetTopStatement(numberOfRecords);
            return "{0}\n{1}".Fi(format, top);
        }

		public static string FormatJson(this string format, params object[] args) {
			ArgumentValidator.ThrowIfNull(format, "format");

			return string.Format(JsonFormatter.Instance, format, args);
		}


		/// <summary>
		/// Alias for <see cref="FormatInvariant" />.
		/// </summary>
		public static string Fi(this string format, params object[] args) {
			return FormatInvariant(format, args);
		}

		/// <summary>
		/// Returns a string with the rightmost cnt characters chopped off
		/// </summary>
		public static string ChopRight(this string s, int cnt) {
			ArgumentValidator.ThrowIfNull(s, "s");
			if (cnt < 0 || s.Length <= cnt) {
				throw new ArgumentException("Invalid cnt value: {0}. String length was {1}".Fi(cnt, s.Length));
			}

			return s.Substring(0, s.Length - cnt);
		}

		public static string ChopRight(this string s, string stringToChop, bool ignoreCase) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(stringToChop, "stringToChop");

			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			return s.EndsWith(stringToChop, comparison) ? s.ChopRight(stringToChop.Length) : s;
		}

		public static string ChopRight(this string s, string stringToChop) {
			return s.ChopRight(stringToChop, false);
		}

       	public static string Mask(this string s, string mask) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(mask, "mask");
       		var m = new StringMask(mask);
       		return m.Apply(s);
       	}

		public static string UnMask(this string s, string mask) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(mask, "mask");
			var m = new StringMask(mask);
			return m.UnMask(s);
		}

		public static string UnPrefix(this string s, string prefix) {
            ArgumentValidator.ThrowIfNull(s, "s");
            ArgumentValidator.ThrowIfNull(prefix, "prefix");
            if (!s.StartsWith(prefix)) {
                return s;
            }
            return s.Substring(prefix.Length, s.Length - prefix.Length);

        }

		/// <summary>
		/// Returns a trimmed, invariant upper case version of this <see cref="string" />.
		/// </summary>
		public static string Normalize(string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return s.Trim().ToUpperInvariant();
		}

		public static string ToNullIfEmpty(string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return 0 == s.Length ? null : s;
		}

		public static string EncodeHtml(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return HttpUtility.HtmlEncode(s);
		}


		public static string ToAbsolute(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return VirtualPathUtility.ToAbsolute(s);
		}

		public static string TryDoSubstitutions(this string s, Dictionary<string, string> keywords) {
			var lambdaKeywords = keywords.ToDictionary(kvp => kvp.Key, kvp => new Func<string, string>(arg => kvp.Value));
			return DoSubstitutions(s, lambdaKeywords, true);
		}

		public static string DoSubstitutions(this string s, Dictionary<string, string> keywords) {
        	var lambdaKeywords = keywords.ToDictionary(kvp => kvp.Key, kvp => new Func<string,string>(arg => kvp.Value));
        	return DoSubstitutions(s, lambdaKeywords, false);
        }

		public static string TryDoSubstitutions(this string s, Dictionary<string, Func<string, string>> keywords) {
			return DoSubstitutions(s, keywords, true);
		}

        public static string DoSubstitutions(this string s, Dictionary<string, Func<string, string>> keywords) {
        	return DoSubstitutions(s, keywords, false);
        }

		internal static string DoSubstitutions(this string s, Dictionary<string, Func<string, string>> keywords, bool ignoreUnknownKeyword) {			
			MatchEvaluator evaluator = m => {
				var keyword = m.Groups["keyword"].Value;
				
				if (!keywords.ContainsKey(keyword)) {
					if (ignoreUnknownKeyword) {
						return m.ToString();
					}
					throw new AssertionViolationException("The unknown keyword $'{0}'$ was used in string. Check the spelling and capitalization.".Fi(keyword));
				}

				var argument = m.Groups["argument"].Success ? m.Groups["argument"].Value : null;

				return keywords[keyword](argument);
			};
			
			var startAt = 0;
			var match = m_keywordRegex.Match(s, startAt);
			while (match.Success) {
				s = m_keywordRegex.Replace(s, evaluator, 1, startAt);
				startAt = Math.Min(match.Index + 1, s.Length);
				match = m_keywordRegex.Match(s, startAt);
			}
			return s;
		}

		public static string EscapeMagicSqlLikeChars(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			// make the common case fast
			if(-1 == s.LastIndexOfAny(MagicSqlLikeChars)) {
				return s;
			}

			for (var i = 0; i < MagicSqlLikeChars.Length; i++) {
				var c = MagicSqlLikeChars[i];
				if (s.LastIndexOf(c) != -1) {
					s = s.Replace(c.ToString(), "[" + c + "]");
				}
			}

			return s;
		}

		public static string CapitalizeFirstLetters(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			// if possible, prevent allocation by checking whether s really needs any capitalization
			var sawWhitespace = true;
			for (var i = 0; i < s.Length; i++) {
				var c = s[i];

				if (Char.IsWhiteSpace(c)) {
					sawWhitespace = true;
					continue;
				}

				if (Char.IsLower(c) && sawWhitespace) {
					return CapitalizeFirstLetters(new StringBuilder(s));
				}

				sawWhitespace = false;
			}

			return s;
		}

		private static string CapitalizeFirstLetters(StringBuilder builder) {
			var sawWhitespace = true;
			for (var i = 0; i < builder.Length; i++) {
				var c = builder[i];

				if (Char.IsWhiteSpace(c)) {
					sawWhitespace = true;
					continue;
				}

				if (Char.IsLower(c) && sawWhitespace) {
					builder[i] = Char.ToUpper(c, CultureInfo.CurrentCulture);
				}

				sawWhitespace = false;
			}

			return builder.ToString();
		}

		public static bool OicStartsWith(this string s, string start) {
			return s.StartsWith(start, StringComparison.OrdinalIgnoreCase);
		}

		public static bool OicEndsWith(this string s, string end) {
			return s.EndsWith(end, StringComparison.OrdinalIgnoreCase);
		}

		public static bool OicContains(this string s, string [] candidates) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(candidates, "candidates");
			return candidates.Any(candidate => s.OicContains(candidate));
		}

		public static bool OicContains(this string s, string candidate) {
			return s.OicIndexOf(candidate) != -1;
		}

		public static int OicIndexOf(this string s, string candidate) {
			ArgumentValidator.ThrowIfNull(s, "s");			
			ArgumentValidator.ThrowIfNull(candidate, "candidate");
			return s.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
		}

		public static int OicLastIndexOf(this string s, string candidate) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(candidate, "candidate");
			return s.LastIndexOf(candidate, StringComparison.OrdinalIgnoreCase);
		}

		public static string OicAfter(this string s, string prefix) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(prefix, "prefix");
			var start = OicLastIndexOf(s, prefix) + prefix.Length;
			return s.Substring(start);
		}

		public static string SpacePascalOrCamel(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return m_pascalOrCamelRegex.Replace(s, "$1 ").ChopRight(1);
		}

		public static string LowerFirstLetter(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return Char.ToLowerInvariant(s[0]) + s.Substring(1);
		}

        public static string Methodize(this string s) {
            return s.Replace('_', ' ').CapitalizeFirstLetters().Replace(" ", string.Empty);
        }

		public static int CountChar(this string s, char c) {
			ArgumentValidator.ThrowIfNull(s, "s");

			var cnt = 0;
			for (var i = 0; i < s.Length; i++) {
				if (s[i] == c) {
					cnt++;
				}
			}

			return cnt;
		}

		public static int? TryToInt(this string s) {
			int result;
			return int.TryParse(s, out result) ? result : (int?) null;
		}

		public static DateTime? TryToDateTime(this string s, IFormatProvider provider, DateTimeStyles styles) {
			ArgumentValidator.ThrowIfNull(provider, "provider");

			DateTime result;
			return DateTime.TryParse(s, provider, styles, out result) ? (DateTime?) result : null;
		}

		public static Double? TryToDouble(this string s) {
			Double result;
			return Double.TryParse(s, out result) ? result : (double?) null;
		}

		public static Decimal? TryToDecimal(this string s) {
			Decimal result;
			return Decimal.TryParse(s, out result) ? result : (decimal?) null;
		}

		public static bool? TryToBool(this string s) {
			bool result;
			return bool.TryParse(s, out result) ? result : (bool?)null;
		}


		public static string StripHtmlElements(this string s) {
			ArgumentValidator.ThrowIfNull(s, "s");

			return -1 == s.IndexOf('<') ? s : m_stripHtmlElements.Replace(s, string.Empty);
		}

		public static bool IsBad(this string s) {
			if (string.IsNullOrEmpty(s)) {
				return true;
			}

            for (var i = 0; i < s.Length; i++) {
				if (!Char.IsWhiteSpace(s[i])) {
					return false;
				}
			}

			return true;
		}

		public static readonly string Clipped = " (...) ";

		public static string Clip(this string s, int cntLeftMostToKeep, int cntRightMostToKeep) {
			ArgumentValidator.ThrowIfNull(s, "s");

			if (cntLeftMostToKeep + cntRightMostToKeep >= s.Length) {
				return s;
			}

			return s.AtMost(cntLeftMostToKeep) + Clipped + s.RightMost(cntRightMostToKeep);
		}

		public static bool IsGood(this string s) {
			return !IsBad(s);
		}
		
		public static bool OicIn(this string s, IEnumerable<string> options) {
			ArgumentValidator.ThrowIfNull(s, "s");
			ArgumentValidator.ThrowIfNull(options, "options");

			return options.Any(o => o.OicEquals(s));
		}

		public static DateTime? TryToDateTime(this string s, IFormatProvider provider) {
			return s.TryToDateTime(provider, DateTimeStyles.None);
		}

		public static string AtMost(this string s, int cnt) {
			ArgumentValidator.ThrowIfNull(s, "s");
			if (cnt <= 0) {
				throw new ArgumentException("cnt must be > 0", "cnt");
			}

			if (s.Length <= cnt) {
				return s;
			}

			return s.Substring(0, cnt);
		}

		public static string RightMost(this string s, int cnt) {
			ArgumentValidator.ThrowIfNull(s, "s");

			if (cnt <= 0) {
				throw new ArgumentException("cnt must be > 0", "cnt");
			}

			if (s.Length <= cnt) {
				return s;
			}

			return s.Substring(s.Length - cnt);
		}


		public static bool HasLetters(this string s) {
			return s.Any(Char.IsLetter);
		}

		public static bool HasWhitespace(this string s) {
			return s.Any(Char.IsWhiteSpace);
		}

		public static bool HasNumbers(this string s) {
			return s.Any(Char.IsNumber);
		}

		public static bool IsAllLetters(this string s) {
			return s.All(Char.IsLetter);
		}

		private static readonly Regex m_keywordRegex = new Regex(
			@"\$(?<keyword>\w+)  ( \( (?<argument>[^)]+) \) )? \$", 
			RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        
		private static readonly Regex m_pascalOrCamelRegex = new Regex(
			@"(?<m> ([A-Z]? [^A-Z]+ (?=$|[A-Z])) | ([A-Z]+ [^a-z]* (?=$|[A-Z][a-z])) )", 
			RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		private static readonly Regex m_stripHtmlElements = new Regex(@"<[\w/][^>]*>", 
			RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
	}
}