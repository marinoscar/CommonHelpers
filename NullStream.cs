// Adapted from http://www.atalasoft.com/cs/blogs/stevehawley/archive/2006/09/15/10867.aspx

using System;
using System.IO;

namespace Common.Helpers {
	public class NullStream : Stream {
		private long m_position;
		private long m_length;

		public override bool CanRead {
			get { return false; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override void Flush() {}

		public override long Length {
			get { return this.m_length; }
		}

		public override long Position {
			get { return this.m_position; }
			set {
				this.m_position = value;
				if (this.m_position > this.m_length) {
					this.m_length = this.m_position;
				}
			}
		}

		public override long Seek(long offset, SeekOrigin origin) {
			long newPosition;

			switch (origin) {
				case SeekOrigin.Current:
					newPosition = Position + offset;
					break;
				case SeekOrigin.End:
					newPosition = Length + offset;
					break;
				default:
					newPosition = offset;
					break;
			}
			if (newPosition < 0) {
				throw new ArgumentException("Attempt to seek before start of stream.");
			}
			Position = newPosition;
			return newPosition;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			throw new NotImplementedException("This stream doesn't support reading.");
		}

		public override int Read(byte[] buffer, int offset, int count) {
			throw new NotImplementedException("This stream doesn't support reading.");
		}

		public override void SetLength(long value) {
			this.m_length = value;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			Seek(count, SeekOrigin.Current);
		}
	}
}