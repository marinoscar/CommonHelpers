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
using System.Runtime.Serialization;
using System.Text;
using Common.Helpers;

namespace Common.Helpers {
	/// <summary>
	/// Represents an exception that has information as why it happened, whether to retry, whether it indicates a bug, and so on.
	/// </summary>
	[Serializable]
	public class CognizantException : ApplicationException {
		/// <summary>
		/// Do NOT use this constructor from code, it's only here so we can be deserialized by .NET.
		/// </summary>
		public CognizantException() {}

		/// <summary>
		/// Initializes a new instance of the <see cref="CognizantException" /> class with a specified error message.
		/// </summary>
		public CognizantException(ExceptionInformation exceptionInformation, string message) : base(message) {
			ArgumentValidator.ThrowIfNull(exceptionInformation, "exceptionInformation");
			
			m_exceptionInformation = exceptionInformation;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CognizantException" /> class with serialized data.
		/// </summary>
		protected CognizantException(SerializationInfo info, StreamingContext context) : base(info, context) {
			if (info != null) {
				m_exceptionInformation	= (ExceptionInformation) info.GetValue("m_exceptionInformation", typeof(ExceptionInformation));
			}
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CognizantException" /> class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public CognizantException(ExceptionInformation exceptionInformation, String message, Exception innerException) : base(message, innerException) {
			ArgumentValidator.ThrowIfNull(exceptionInformation, "exceptionInformation");
			
			m_exceptionInformation = exceptionInformation;
		}

		/// <summary>
		/// Gets the <see cref="ExceptionInformation" /> object which describes this <see cref="CognizantException" />.
		/// </summary>
		public ExceptionInformation ExceptionInformation {
			get { return this.m_exceptionInformation; }
		}

		/// <summary>
		/// Adds <see cref="ExceptionInformation" /> to a serialized <see cref="CognizantException" />.
		/// </summary>
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData (info, context);

			if (info != null) {
				info.AddValue("m_exceptionInformation", this.ExceptionInformation);
			}
		}
		
		private readonly ExceptionInformation m_exceptionInformation;
	}
}
