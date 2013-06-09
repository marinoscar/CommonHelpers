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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Common.Helpers {
	public class SqlFormatter : IFormatProvider, ICustomFormatter {
		public readonly static SqlFormatter Instance = new SqlFormatter();


		public object GetFormat(Type formatType) {
			return this;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider) {
			return Format(format, arg);
		}

		public static readonly string[] StringComparisonOperators = new[] {"equals", "startsWith", "endsWith", "contains"};

		public static string Format(string format, object o) {
			var prefix = format == "equals" ? "= " : string.Empty;

			if (o.IsNullOrDbNull()) {
				if (StringComparisonOperators.OicContains(format)) {
					return "IS NULL";
				}

				return "NULL";
			}

			if (o is DateTime) {
				return prefix + "'{0:yyyy-MM-dd HH:mm:ss.fff}'".Fi(o);
			}

			if (o is string) {
				var s = (string) o;

                if (format == "verbatim") {
                    return s;
                }

				if (format == "name") {
					return "[{0}]".Fi(s.Replace("]", "]]"));
				}

				s = s.Replace("'", "''");
				if (format == "startsWith") {
					s = s.EscapeMagicSqlLikeChars();
					return "LIKE '{0}%'".Fi(s);
				}

				if (format == "endsWith") {
					s = s.EscapeMagicSqlLikeChars();
					return "LIKE '%{0}'".Fi(s);
				}

				if (format == "contains") {
					s = s.EscapeMagicSqlLikeChars();
					return "LIKE '%{0}%'".Fi(s);
				}

				return prefix + "'{0}'".Fi(s);
			}

			if (o is bool) {
				return prefix + ((bool)o ? "1" : "0");
			}

			if (o is ICollection<byte>) {
				var bytes = (o as ICollection<byte>);
				var builder = new StringBuilder(prefix + "0x", bytes.Count * 2 + 8);

				foreach (var b in bytes) {
					builder.Append(b.ToHex());
				}

				return builder.ToString();
			}

			if (o is IEnumerable) {
				var builder = new StringBuilder(prefix, 32);
				builder.Append("(");

				foreach (var item in (IEnumerable)o) {
					builder.AppendFormat("{0},", Format(null, item));
				}

				if (1 == builder.Length) {
					builder.Append("NULL");
				} else {
					builder.Length -= 1;
				}
				
				builder.Append(")");
				return builder.ToString();
			}

			if (o is int) {
				return prefix + ((int)o).ToString(CultureInfo.InvariantCulture);
			}

			if (o is double) {
				return prefix + ((double)o).ToString(CultureInfo.InvariantCulture);
			}

			return prefix + "{0}".Fi(o);
		}
	}
}
