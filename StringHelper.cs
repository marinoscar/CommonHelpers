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
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Common.Helpers {
	public class StringHelper {
		private static readonly char[] m_escapeChars = new[] { '_', '.', '$' };

		public static string EscapeChar(string s, char charToEliminate) {
			ArgumentValidator.ThrowIfNull(s, "s");

			char escapeChar;
			char substituteChar;

			GetEscapeAndEncodingChars(charToEliminate, out escapeChar, out substituteChar);

			int i;
			for (i = 0; i < s.Length; i++) {
				if (s[i] == charToEliminate || s[i] == escapeChar) {
					break;
				}
			}

			if (i == s.Length) {
				return s;
			}

			var sb = new StringBuilder(s.Substring(0, i), s.Length + 16);

			for (; i < s.Length; i++) {
				var c = s[i];

				if (c == charToEliminate) {
					sb.Append(escapeChar);
					sb.Append(substituteChar);
				} else if (c == escapeChar) {
					sb.Append(escapeChar, 2);
				} else {
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		public static string UnescapeChar(string s, char eliminatedChar) {
			ArgumentValidator.ThrowIfNull(s, "s");

			char escapeChar;
			char substituteChar;

			GetEscapeAndEncodingChars(eliminatedChar, out escapeChar, out substituteChar);

			if (-1 == s.IndexOf(escapeChar)) {
				return s;
			}

			var sb = new StringBuilder(s.Length);

			for (var i = 0; i < s.Length; i++) {
				var c = s[i];

				var next = i + 1;

				if (c != escapeChar || next == s.Length) {
					sb.Append(c);
					continue;
				}

				if (s[next] == escapeChar) {
					sb.Append(escapeChar);
					i++;
				} else if (s[next] == substituteChar) {
					sb.Append(eliminatedChar);
					i++;
				} else {
					var msg = ("Invalid escaped string '{0}'. Position {1} has escape character, but it is not followed by "
						+ "another escape character or by the substitute character").Fi(s, i);
					throw new AssertionViolationException(msg);
				}
			}

			return sb.ToString();
		}


		private static void GetEscapeAndEncodingChars(char charToEliminate, out char escapeChar, out char encodingChar) {
			var i = m_escapeChars[0] == charToEliminate ? 1 : 0;
			escapeChar = m_escapeChars[i];
			i++;

			if (m_escapeChars[i] == charToEliminate) {
				i++;
			}
			encodingChar = m_escapeChars[i];
		}

		public static string SerializeExpression(Expression<Action> action) {
			ArgumentValidator.ThrowIfNull(action, "action");

			if (!(action.Body is MethodCallExpression)) {
				ThrowInvalidExpression(action);
			}

			var call = (MethodCallExpression)action.Body;
			if (call.Object != null) {
				ThrowInvalidExpression(action);
			}

			var sb = new StringBuilder(128);
			var assembly = EscapeChar(call.Method.DeclaringType.AssemblyQualifiedName, '|');
			sb.AppendFormat("{0}|", assembly);
			sb.AppendFormat("{0}|", EscapeChar(call.Method.Name, '|'));

			for (var i = 0; i < call.Arguments.Count; i++) {
				var value = ValueFromExpression(action, call.Arguments[i]);
				SerializeArgument(action, sb, value);
			}

			sb.Length -= 1;
			return sb.ToString();
		}

		private static object ValueFromExpression(Expression<Action> action, Expression exp) {
			var constant = exp as ConstantExpression;
			if (null != constant) {
				return constant.Value;
			}

			var memberAccess = exp as MemberExpression;
			if (memberAccess != null) {
				object target = null;
				var targetExpression = (memberAccess.Expression as ConstantExpression);
				if (targetExpression != null) {
					target = targetExpression.Value;
				}

				var field = (memberAccess.Member as FieldInfo);
				if (field != null) {
					return field.GetValue(target);
				}

				var property = (memberAccess.Member as PropertyInfo);
				if (property != null) {
					return property.GetValue(target, null);	
				}
			}

			ThrowInvalidExpression(action);
			return null;
		}

		public static void InvokeSerializedExpression(string exp) {
			ArgumentValidator.ThrowIfNullOrEmpty(exp, "exp");

			var parts = exp.Split('|');
			if (parts.Length < 2 || parts.Length.IsOdd()) {
				var msg = "{0} is not a valid serialized expression".Fi(exp);
				throw new ArgumentException(msg, "exp");
			}

			var type = Type.GetType(parts[0]);
			var method = type.GetMethod(parts[1], BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if (null == method) {
				var msg = ("Could not find method '{0}' on type '{1}' as serialized in expression '{2}'. This is either a bug in the serializer (can't be!) "
				           + "or perhaps you're using a different version of type '{1}'.").Fi(parts[1], type.FullName, exp);
				throw new AssertionViolationException(msg);
			}

			var argCount = (parts.Length - 2)/2;
			var args = new object[argCount];

			for (var i = 0; i < argCount; i++) {
				DeserializeArgument(args, parts, i);
			}

			method.Invoke(null, args);
		}

		private static void DeserializeArgument(object[] args, string[] parts, int idxArgument) {
			var idxToken = 2 + idxArgument*2;
			var typeCode = (TypeCode) Enum.Parse(typeof(TypeCode), parts[idxToken]);

			var value = UnescapeChar(parts[idxToken + 1], '|');
			args[idxArgument] = Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
		}

		private static void SerializeArgument(Expression<Action> action, StringBuilder builder, object value) {
			var typeCode = Convert.GetTypeCode(value);

			if (TypeCode.Object == typeCode) {
				var msg = ("Expression {0} uses an argument of type {1}, which is not a well-known .net type with a corresponding "
						   + "System.TypeCode. You cannot use this as an argument.").Fi(action, value.GetType().FullName);
				throw new ArgumentException(msg);
			}

            var s = string.Format(CultureInfo.InvariantCulture, "{0}", value);
			builder.AppendFormat("{0}|{1}|", typeCode, EscapeChar(s, '|'));
		}

		private static void ThrowInvalidExpression(Expression<Action> action) {
			var msg = ("The expression {0} is not allowed as a Lambda task. The expression must be a call to a static method using only "
				+ "primitive types as parameters. Null values are not allowed in the parameters.").Fi(action);
			throw new ArgumentException(msg, "action");
		}
	}
}