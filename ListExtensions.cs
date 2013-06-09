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
using System.Linq;

namespace Common.Helpers {
	public static class ListExtensions {
		public static int OicIndexOfOrDie(this List<string> list, string item) {
			ArgumentValidator.ThrowIfNull(list, "list");

			for (var i = 0; i < list.Count; i++) {
				if (list[i].OicEquals(item)) {
					return i;
				}
			}

			throw new KeyNotFoundException("Cannot find an item maching {0} in list".Fi(item));
		}

		public static int IndexOfOrDie<T>(this List<T> list, T item) {
			ArgumentValidator.ThrowIfNull(list, "list");

			for (var i = 0; i < list.Count; i++) {
				if (list[i].Equals(item)) {
					return i;
				}
			}

			throw new KeyNotFoundException("Cannot find an item maching {0} in list".Fi(item));
		}

		public static bool OicContains(this List<string> list, string item) {
			ArgumentValidator.ThrowIfNull(list, "list");

			for (var i = 0; i < list.Count; i++) {
				if (list[i].OicEquals(item)) {
					return true;
				}
			}

			return false;
		}

		public static T GetLast<T>(this List<T> list) {
			ArgumentValidator.ThrowIfNull(list, "list");

			if (0 == list.Count) {
				throw new InvalidOperationException("List is empty");
			}

			return list[list.Count - 1];
		}

		public static List<T> OnEach<T>(this List<T> list, Action<T> doSomething) {
			ArgumentValidator.ThrowIfNull(list, "list");
			ArgumentValidator.ThrowIfNull(doSomething, "doSomething");

			for (int i = 0; i < list.Count; i++) {
				doSomething(list[i]);
			}

			return list;
		}

		public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable {
			return listToClone.Select(item => (T)item.Clone()).ToList();
		}


	}
}
