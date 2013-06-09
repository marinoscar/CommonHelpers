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
using System.Text;

namespace Common.Helpers {
	/// <summary>
	/// Encapsulates information about an exception.
	/// </summary>
	[Serializable]
	public class ExceptionInformation {
		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionInformation"/> class.
		/// </summary>
		public ExceptionInformation(Boolean isBug, Boolean mayRetry) : this(isBug, mayRetry, false, false) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionInformation"/> class.
		/// </summary>
		public ExceptionInformation(bool isBug, bool mayRetry, bool isUnknown, bool shouldShutdown) {
			m_isBug = isBug;
			m_mayRetry = mayRetry;
			m_isUnknown = isUnknown;
			m_shouldShutdown = shouldShutdown;
		}


		/// <summary>
		/// Gets a value indicating whether the exception was likely caused by a bug.
		/// </summary>
		public Boolean IsBug {
			get { return m_isBug; }
		}

		/// <summary>
		/// Gets a value indicating whether the operation that led to the exception may be retried without changes.
		/// </summary>
		public Boolean MayRetry {
			get { return m_mayRetry; }
		}

		/// <summary>
		/// Gets a value indicating whether the exception is unknown.
		/// </summary>
		public Boolean IsUnknown {
			get { return m_isUnknown; }
		}

		public bool ShouldShutdown {
			get { return this.m_shouldShutdown; }
		}

		/// <summary>
		/// Ready made <see cref="ExceptionInformation" /> for exceptions caused by bugs.
		/// </summary>
		public static readonly ExceptionInformation Bug = new ExceptionInformation(true, false);
		
		/// <summary>
		/// Ready made <see cref="ExceptionInformation" /> for exceptions caused by an action that should not be retried without modification.
		/// </summary>
		public static readonly ExceptionInformation NoRetry = new ExceptionInformation(false, false);

		/// <summary>
		/// Ready made <see cref="ExceptionInformation" /> for unknown exceptions.
		/// </summary>
		public static readonly ExceptionInformation Unknown = new ExceptionInformation(false, false);

		/// <summary>
		/// Ready made <see cref="ExceptionInformation" /> for virtual machine errors in the .net VM (ie, bad!)
		/// </summary>
		public static readonly ExceptionInformation VirtualMachineError = new ExceptionInformation(false, true, false, true);

		/// <summary>
		/// Ready made <see cref="ExceptionInformation" /> for temporary exceptions where a retry is warranted
		/// </summary>
		public static readonly ExceptionInformation Retry = new ExceptionInformation(false, true);

		
		private readonly Boolean m_mayRetry;
		private readonly Boolean m_isBug;
		private readonly Boolean m_isUnknown;
		private readonly Boolean m_shouldShutdown;
	}
}
