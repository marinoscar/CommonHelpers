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

namespace Common.Helpers {
	/// <summary>
	/// This exception is thrown when a concurrency problem is detected.
	/// </summary>
	[Serializable]
	public class ConcurrencyException : CognizantException {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
		/// </summary>
		public ConcurrencyException(String message) : base(ExceptionInformation.NoRetry, message) {}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrencyException" /> class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public ConcurrencyException(String message, Exception innerException) : base(ExceptionInformation.NoRetry, message, innerException) {}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrencyException" /> class with serialized data.
		/// </summary>
		protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context) {}
	}
}
