using System;
using Common.Helpers;
using Newtonsoft.Json;

namespace Common.Helpers {
	public class JsonLiteral {
		public JsonLiteral(string literal) {
			this.Literal = literal;
		}

		public readonly string Literal;
	}
}