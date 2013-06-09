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
	public static class EnumerableExtensions {
		public static IEnumerable<IEnumerable<T>> InChunksOf<T>(this IEnumerable<T> source, int chunkSize) {
			ArgumentValidator.ThrowIfNull(source, "source");

			var chunk = new List<T>(chunkSize);
			var i = 0;

			foreach (var e in source) {
				chunk.Add(e);
				i++;

				if (i % chunkSize == 0) {
					yield return chunk;
					chunk.Clear();
				}
			}

			if (chunk.Count > 0) {
				yield return chunk;
			}
		}

		public static void InChunksOf<T>(this IEnumerable<T> source, int chunkSize, Action<IEnumerable<T>> doSomething) {
			ArgumentValidator.ThrowIfNull(doSomething, "doSomething");
			foreach (var chunk in source.InChunksOf(chunkSize)) {
				doSomething(chunk);
			}
		}

		public static void Each<T>(this IEnumerable<T> source, Action<T> doSomething) {
			ArgumentValidator.ThrowIfNull(source, "source");
			ArgumentValidator.ThrowIfNull(doSomething, "doSomething");

			foreach (var e in source) {
				doSomething(e);
			}
		}

	}
}
