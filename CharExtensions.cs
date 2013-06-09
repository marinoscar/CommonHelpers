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
using System.Collections.ObjectModel;
using System.Text;

namespace Common.Helpers {
	public static class CharExtensions {
		public static readonly ReadOnlyCollection<string> Low32CharNames = new ReadOnlyCollection<string>(new[] {
				"NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", 
				"BS", "TAB", "LF", "VTAB", "FF", "CR", "SO", "SI",
				"DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB",
				"CAN", "EM", "SUB", "ESC", "FS", "GS", "RS", "US", "SPACE"
			});
		
		public static String GetDescription(this char c) {
			String printed;
			
			if (c <= 32) {
				printed = "<{0}>".Fi(Low32CharNames[c]);
			} else if(c > 127) {
				printed = String.Empty;
			} else {
				printed = "'{0}' ".Fi(c);
			}

			return printed + " (U+{0:x4})".Fi((Int32) c);
		}
		
		public static bool OicEquals(this char c1, char c2) {
			return char.ToLowerInvariant(c1) == char.ToLowerInvariant(c2);
		}
	}
}
