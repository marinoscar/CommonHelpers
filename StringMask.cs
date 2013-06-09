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
	public class StringMask {
		#region Public members

		public readonly string Mask;

		public StringMask(string mask) {
			this.Mask = mask;
		}

		public string UnMask(string value) {
			ArgumentValidator.ThrowIfNull(value, "value");
			ArgumentValidator.ThrowIfTrue(value.Length > this.Mask.Length, "Argument length must not exceed mask's length.");
			return value.Where((t, i) => IsMaskChar(this.Mask[i])).Aggregate("", (current, t) => current + t);
		}

		public string Apply(string value) {
			ArgumentValidator.ThrowIfNull(value, "value");
			var result = "";
			var j = 0;
			for(var i = 0; i < this.Mask.Length; i++) {
				if (IsMaskChar(this.Mask[i]) && j < value.Length) {
					result += value[j++];
					continue;
				}
				result += IsMaskChar(Mask[i]) ? ' ' : Mask[i];
			}
			return result.TrimEnd();
		}

		#endregion

		#region Internal and Private members

		private static readonly List<char> m_wildcards = new List<char> {'#', '?', '*'};

		private static bool IsMaskChar(char c) {
			return m_wildcards.Contains(c);
		}

		#endregion
	}
}