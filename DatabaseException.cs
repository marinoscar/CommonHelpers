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
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;

namespace Common.Helpers {
	/// <summary>
	/// This exception is thrown when a concurrency problem is detected.
	/// </summary>
	[Serializable]
	public class DatabaseException : CognizantException {
		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseException" /> class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public DatabaseException(String message, SqlException ex) : base(SqlErrorCodes.AnalyzeException(ex), message, ex) {
			this.IsUniquenessViolation = SqlErrorCodes.IsUniquenessViolation(ex);
		}

		public bool IsUniquenessViolation;

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseException" /> class with serialized data.
		/// </summary>
		protected DatabaseException(SerializationInfo info, StreamingContext context) : base(info, context) {
			if (info != null) {
				this.IsUniquenessViolation = (bool)info.GetValue("IsUniquenessViolation", typeof(bool));
			}
		}

		/// <summary>
		/// Adds <see cref="ExceptionInformation" /> to a serialized <see cref="CognizantException" />.
		/// </summary>
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);

			if (info != null) {
				info.AddValue("IsUniquenessException", this.IsUniquenessViolation);
			}
		}
	}
}
