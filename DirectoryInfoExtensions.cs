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
using System.IO;
using System.Linq;
using System.Text;

namespace Common.Helpers {
	public static class DirectoryInfoExtensions {
		public static FileInfo GetFileOrDie(this DirectoryInfo dir, string fileName) {
			ArgumentValidator.ThrowIfNull(dir, "dir");
			ArgumentValidator.ThrowIfNullOrEmpty(fileName, "fileName");

			var filez = dir.GetFiles(fileName);
			if (0 == filez.Length) {
				var msg = "Failed to find file '{0}' in folder '{1}'".Fi(fileName, dir.FullName);
				throw new FileNotFoundException(msg);
			}

			if (1 < filez.Length) {
				var msg = "Expected to find one file name '{0}' in folder '{1}', but multiple matches were found!".Fi(fileName, dir.FullName);
				throw new AssertionViolationException(msg);
			}

			return filez[0];
		}
	}
}
