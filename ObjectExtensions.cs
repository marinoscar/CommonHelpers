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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Helpers {

    public static class ObjectExtensions {
		public static string ToJson(this object o) {
			ArgumentValidator.ThrowIfNull(o, "o");
            
			if (o is Type) {
				var t = o as Type;
				if (t.IsEnum) {
					return EnumExtensions.GetNamesAndValues(t, true).ToJson();
				}
			}

			return JsonConvert.SerializeObject(o, DateConverter, JsonLiteralConverter);
		}

		public static readonly JsonConverter DateConverter = new PainlessJsonDateConverter();
		public static readonly JsonConverter JsonLiteralConverter = new JsonLiteralConverter();

		public static string ToSql(this object o) {
			ArgumentValidator.ThrowIfNull(o, "o");

			return SqlFormatter.Format(null, o);
		}

		public static bool IsNullOrDbNull(this object o) {
			return null == o || Convert.IsDBNull(o);
		}

		public static T Or<T> (this object o, T alternative) {
			if (o.IsNullOrDbNull()) {
				return alternative;
			}

			return (T) (o is T ? o : Convert.ChangeType(o, typeof (T)));
		}

        public static T Or<T> (this T o,Func<T,bool> test, T alternative) {
            return test(o) ? o : alternative;
        }

		public static T Do<T>(this T o, Action<T> doSomething) {
			doSomething(o);
			return o;
		}

		public static T LambdaOrSelf<T>(this object o) {
			ArgumentValidator.ThrowIfNull(o, "o");

			var t = o.GetType();
			var m = t.GetMethod("Invoke");
			if (null == m) {
				return (T)o;
			}

			var pi = m.GetParameters();
			var rt = m.ReturnType;
			if (pi.Length == 0 && rt != typeof(void)) {
				return (T)m.Invoke(o, new object[] { });
			}
			throw new ArgumentException();
		}
    }
}