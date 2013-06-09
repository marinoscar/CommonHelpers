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

namespace Common.Helpers {
	/// <summary>
	/// Integer hasher based on the Jenkins96 hasher, as described in http://bretm.home.comcast.net/~bretm/hash/7.html
	/// </summary>
	/// <remarks>
	/// <para>This version has zero heap allocations and works directly on integers. I unit tested this a fair bit to make sure results matched the implementation 
	/// mentioned above. Everything checked out for many runs of random data
	/// of various lengths, so we're good. 
	/// </para>
	/// </remarks>
	public struct IntegerHasher {
		private uint a, b, c;
		private uint m_len;
		private bool m_finished;
		private bool m_inited;
		private int m_move;

		void Mix() {
			a -= b; a -= c; a ^= (c >> 13);
			b -= c; b -= a; b ^= (a << 8);
			c -= a; c -= b; c ^= (b >> 13);
			a -= b; a -= c; a ^= (c >> 12);
			b -= c; b -= a; b ^= (a << 16);
			c -= a; c -= b; c ^= (b >> 5);
			a -= b; a -= c; a ^= (c >> 3);
			b -= c; b -= a; b ^= (a << 10);
			c -= a; c -= b; c ^= (b >> 15);
		}

		public void Hash(int data) {
			this.Hash((uint) data);
		}

		public void Hash(uint data) {
			if (!this.m_inited) {
				a = b = 0x9e3779b9;
				this.m_inited = true;
			}

			if(0 == this.m_move) {
				a += data;
			} else if (1 == this.m_move) {
				b += data;
			} else {
				c += data;
				this.Mix();
			}

			this.m_move++;
			this.m_move %= 3;
			this.m_len += 4;
		}

		public uint Finish() {
			if (this.m_finished) {
				throw new InvalidOperationException("You can only call Finish() once");
			}

			this.m_finished = true;
			c += this.m_len;
			this.Mix();
			return c;
		}
	}
}