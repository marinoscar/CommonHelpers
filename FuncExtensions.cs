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
using System.Threading;

namespace Common.Helpers {
	public class FuncReturnWrapper<T> {
		public FuncReturnWrapper(Func<T> func) {
			ArgumentValidator.ThrowIfNull(func, "func");

			this.m_func = func;
		}

		public void Run() {
			try {
				this.m_return = this.m_func();	
			} catch (Exception ex) {
				this.Exception = ex;
			}
		}

		private T m_return;
		private readonly Func<T> m_func;

		public T Return {
			get { return this.m_return; }
		}

		public bool HasException {
			get { return this.Exception != null; }
		}

		public Exception Exception { get; private set; }
	}


	public static class FuncExtensions {
		public static readonly Func<bool> True = () => true;
		public static readonly Func<bool> False = () => false;

        public static Func<bool> GetImmutable(bool v) {
            return v ? FuncExtensions.True : FuncExtensions.False;
        }

	    public static T ExecuteWithTimeout<T>(this Func<T> delegateToRun, TimeSpan timeout) {
			ArgumentValidator.ThrowIfNull(delegateToRun, "delegateToRun");

			var w = new FuncReturnWrapper<T>(delegateToRun);
			var t = new Thread(w.Run);

			t.Start();
			bool terminated = t.Join(timeout);
			if (!terminated) {
				t.Abort();
				throw new TimeoutException("Timeout of {0} for running delegate {1} has expired".Fi(timeout, delegateToRun));
			}

			if (w.HasException) {
				// SHOULD: somehow preserve the original (real) StackTrace. If we throw a generic Exception using
				// w.Exception as the inner, we'd lose the expression type.
				// The current solution is also bad because it's obscure since the caller won't look in the Data. But
				// it's better than nothing.
				w.Exception.Data["originalStackTrace"] = w.Exception.StackTrace;

				throw w.Exception;
			}

			return w.Return;
		}

		public static string EmptyString() {
			return string.Empty;
		}
	}
}
