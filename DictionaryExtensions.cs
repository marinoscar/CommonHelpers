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
using System.Collections.Generic;
using System.Linq;

namespace Common.Helpers {
    public static class DictionaryExtensions {
		public static TValue TryGet<TKey,TValue>(this Dictionary<TKey,TValue> d, TKey key) {
			ArgumentValidator.ThrowIfNull(d, "d");

			TValue v;
			d.TryGetValue(key, out v);
			return v;
		}

		public static T Get<T>(this Dictionary<string, object> d, string key) {
			ArgumentValidator.ThrowIfNull(d, "d");
			ArgumentValidator.ThrowIfNullOrEmpty(key, "key");

			return (T)d[key];
		}

		public static T TryGet<T>(this Dictionary<string, object> d, string key) {
			ArgumentValidator.ThrowIfNull(d, "d");
			ArgumentValidator.ThrowIfNullOrEmpty(key, "key");

			object o;
			return d.TryGetValue(key, out o) ? (T) o : default(T);
		}

		public static string Get(this Dictionary<string, string> d, string firstKey, string secondKey) {
			ArgumentValidator.ThrowIfNull(d, "d");
			ArgumentValidator.ThrowIfNullOrEmpty(firstKey, "firstKey");
			ArgumentValidator.ThrowIfNullOrEmpty(secondKey, "secondKey");

			string v;
			return d.TryGetValue(firstKey, out v) ? v : d[secondKey];
		}

		public static void DuplicateKeys(this Dictionary<string,object> d, List<KeyValuePair<string,string>> copies) {
			ArgumentValidator.ThrowIfNull(d, "d");
			ArgumentValidator.ThrowIfNull(copies, "copies");

			foreach (var pair in copies) {
				if (!d.ContainsKey(pair.Key)) {
					continue;
				}

				d[pair.Value] = d[pair.Key];
			}
		}

		public static Dictionary<string, object> ApplyIf(this Dictionary<string, object> d, IDictionary<string, object> defaults) {
			ArgumentValidator.ThrowIfNull(defaults, "defaults");
			foreach (var o in defaults.Where(o => !d.ContainsKey(o.Key))) {
        		d.Add(o.Key, o.Value);
        	}
			return d;
		}
    }
}