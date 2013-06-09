/*

This class was initially based on code by Stefan Delmarco, http://www.fotia.co.uk/

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
using System.IO;

namespace Common.Helpers {
	public class VarBinaryStream : Stream {
		private long m_position;
		private readonly VarBinaryHelper m_varBinaryHelper;

		public VarBinaryStream(VarBinaryHelper source) {
			this.m_position = 0;
			this.m_varBinaryHelper = source;
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override long Length {
			get { return this.m_varBinaryHelper.Length; }
		}

		public override long Position {
			get { return this.m_position; }
			set { this.Seek(value, SeekOrigin.Begin); }
		}

		public override void Flush() {}

		public override long Seek(long offset, SeekOrigin origin) {
			long newPosition;

			if (SeekOrigin.Begin == origin) {
				newPosition = offset;
			} else if (SeekOrigin.Current == origin) {
				newPosition = this.m_position + offset;
			} else {
				newPosition = this.Length - offset;
			}

			if (newPosition < 0 || newPosition >= this.Length) {
				throw new ArgumentException("The new proposed position would be {0}, which is outside the valid bounds between 0 and {1}".Fi(newPosition, this.Length-1));
			}

			this.m_position = newPosition;
			return this.m_position;
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			ArgumentValidator.ThrowIfNull(buffer, "buffer");

			var data = this.m_varBinaryHelper.Read(Position, count);

			Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
			this.m_position += data.Length;
			return data.Length;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			ArgumentValidator.ThrowIfNull(buffer, "buffer");

			if (offset != 0) {
				var msg = "This class does not support offsets different than zero because we must pass your buffer to a SqlCommand, which "
					+ "does not understand buffer offsets. Duplicating buffers is too wasteful.";

				throw new ArgumentException(msg, "offset");
			}
	
			if (count < 0 || count > buffer.Length) {
				throw new ArgumentOutOfRangeException("count");
			}

			this.m_varBinaryHelper.Write(buffer, count, this.m_position, count);
			this.m_position += count;
		}

		protected override void Dispose(bool disposing) {
			if (!disposing) {
				if (this.m_varBinaryHelper != null) {
					this.m_varBinaryHelper.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}