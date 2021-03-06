﻿/*

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
using System.Runtime.Serialization;
using Common.Helpers;

namespace Common.Helpers {
	/// <summary>
	/// Thrown when a resource (eg, database server, network connection) is not available
	/// </summary>
	[Serializable]
	public class ResourceNotAvailableException : CognizantException {
		/// <summary>
		/// Do NOT use this constructor from code, it's only here so we can be deserialized by .NET.
		/// </summary>
		public ResourceNotAvailableException() {}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceNotAvailableException"/> class.
		/// </summary>
		public ResourceNotAvailableException(string message) : base(Common.Helpers.ExceptionInformation.Retry, message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceNotAvailableException"/> class.
		/// </summary>
		public ResourceNotAvailableException(string message, Exception innerException) : base(ExceptionInformation.Bug, message, innerException) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceNotAvailableException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
		protected ResourceNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context) {}
	}
}