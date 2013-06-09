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
using System.Data;
using System.Data.SqlClient;

namespace Common.Helpers {
	public static class IDataRecordExtensions {
		public static int GetOrdinalOrMinusOne(this IDataRecord r, string columnName) {
			ArgumentValidator.ThrowIfNull(r, "r");
			ArgumentValidator.ThrowIfNullOrEmpty(columnName, "columnName");

			var cntFields = r.FieldCount;
			for (var i = 0; i < cntFields; i++) {
				if (r.GetName(i).OicEquals(columnName)) {
					return i;
				}
			}

			return -1;
		}

		public static bool HasColumn(this IDataRecord r, string columnName) {
			ArgumentValidator.ThrowIfNull(r, "r");
			ArgumentValidator.ThrowIfNullOrEmpty(columnName, "columnName");

			return r.GetOrdinalOrMinusOne(columnName) != -1;
		}

		public static T TryGet<T>(this IDataRecord r, string columnName) {
			ArgumentValidator.ThrowIfNull(r, "r");
			ArgumentValidator.ThrowIfNullOrEmpty(columnName, "columnName");

			var idx = r.GetOrdinalOrMinusOne(columnName);
			if (-1 == idx) {
				return default(T);
			}

			var v = r[idx];
			return Convert.IsDBNull(v) ? default(T) : (T) v;
		}
	}
}
