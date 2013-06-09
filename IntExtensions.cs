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

namespace Common.Helpers {
	public static class IntExtensions {
		public static List<int> To(this int from, int to) {
			var cnt = Math.Abs(to - from) + 1;
			var step = from < to ? 1 : -1;

			var result = new List<int>(cnt);
			for (var i = 0; i < cnt; i++, from += step) {
				result.Add(i);
			}

			return result;
		}

		public static List<T> Of<T>(this int cnt, Func<int,T> generate) {
			ArgumentValidator.ThrowIfNull(generate, "generate");

			var list = new List<T>(cnt);

			for (var i = 0; i < cnt; i++) {
				list.Add(generate(i));
			}

			return list;
		}

		public static void Times(this int cnt, Action action) {
			for (var i = 0; i < cnt; i++) {
				action();
			}
		}

		public static void Times(this int cnt, Action<int> action) {
			for (var i = 0; i < cnt; i++) {
				action(i);
			}
		}

		public static bool IsOdd(this int i) {
			return (i%2) != 0;
		}

		public static bool IsEven(this int i) {
			return !i.IsOdd();
		}
	}
}
