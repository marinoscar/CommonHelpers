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
	public struct DateRange {
		public DateRange(DateTime start, DateTime end) {
			this.Start = start;
			this.End = end;
		}

		public bool Covers(DateTime instant) {
			return this.Start <= instant && instant < this.End;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (obj.GetType() != typeof (DateRange)) {
				return false;
			}
			return Equals((DateRange) obj);
		}

		public DateTime Start;
		public DateTime End;

		public bool Equals(DateRange other) {
			return other.Start.Equals(this.Start) && other.End.Equals(this.End);
		}

		public override int GetHashCode() {
			unchecked {
				return (this.Start.GetHashCode()*397) ^ this.End.GetHashCode();
			}
		}
	}
}
