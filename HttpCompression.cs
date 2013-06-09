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
using System.IO;
using System.Web;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Common.Helpers {
	public class HttpCompression {
		static HttpCompression() {
			None = new HttpCompression("None");
			Gzip = new HttpCompression("gzip");
			Deflate = new HttpCompression("deflate");
		}

		public override string ToString() {
			return this.Name;
		}

		public void AddResponseHeader(HttpResponse response) {
			ArgumentValidator.ThrowIfNull(response, "response");

			if (this.IsNone) {
				return;
			}

			response.AppendHeader("Content-Encoding", this.ToString());
		}

		public Stream WrapStream(Stream baseStream) {
			ArgumentValidator.ThrowIfNull(baseStream, "baseStream");

			if (this.IsNone) {
				return baseStream;
			}

			if (this.Name == "gzip") {
				var gzipStream = new GZipOutputStream(baseStream, 32768);
				gzipStream.SetLevel(9);
				return gzipStream;
			}

			if (this.Name == "deflate") {
				return new DeflaterOutputStream(baseStream, new Deflater(Deflater.BEST_COMPRESSION, true), 32768);
			}

			throw new AssertionViolationException("Unknown HttpCompression Name: " + this.Name);
		}

		public bool IsNone {
			get { return this.Name == "None"; }
		}

		public static void CompressResponse(HttpRequest request, HttpResponse response) {
			ArgumentValidator.ThrowIfNull(request, "request");
			ArgumentValidator.ThrowIfNull(response, "response");

			var c = GetBestCompressionFor(request);
			c.AddResponseHeader(response);
			response.Filter = c.WrapStream(response.Filter);
		}

		public static HttpCompression GetBestCompressionFor(HttpRequest request) {
			ArgumentValidator.ThrowIfNull(request, "request");

			var acceptEncoding = request.Headers["Accept-Encoding"];
			if (acceptEncoding.IsBad()) {
				return None;
			}

			if (acceptEncoding.Contains("deflate") || acceptEncoding == "*") {
				return Deflate;
			}

			if (acceptEncoding.Contains("gzip")) {
				return Gzip;
			}

			return None;
		}

		private HttpCompression(string name) {
			ArgumentValidator.ThrowIfNullOrEmpty(name, "name");

			this.Name = name;
		}
        
		public readonly string Name;
		public static readonly HttpCompression None;
		public static readonly HttpCompression Gzip;
		public static readonly HttpCompression Deflate;
	}
}