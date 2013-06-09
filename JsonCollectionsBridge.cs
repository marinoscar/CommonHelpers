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
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace Common.Helpers {
	// MUST: handle explicit nulls in json, currently leads to ThrowOnUnknownToken
	public class JsonCollectionsBridge {
		public JsonCollectionsBridge(
			Func<IDictionary<string,object>> createDictionary, 
			Func<IDictionary<string,object>,IDictionary<string,object>> wrapDictionary,
			Func<List<object>,ICollection<object>> wrapJavascriptArrays
		) {
			ArgumentValidator.ThrowIfNull(createDictionary, "createDictionary");
			ArgumentValidator.ThrowIfNull(wrapDictionary, "wrapDictionary");
			ArgumentValidator.ThrowIfNull(wrapJavascriptArrays, "wrapJavascriptArrays");

			this.CreateDictionary = createDictionary;
			this.WrapDictionary = wrapDictionary;
			this.WrapJavascriptArrays = wrapJavascriptArrays;
		}

		public static readonly JsonCollectionsBridge ReadOnly = new JsonCollectionsBridge(
			() => new Dictionary<string, object>(StringComparer.Ordinal),
			d => new ReadOnlyDictionary<string, object>(d),
			l => new ReadOnlyCollection<object>(l));

		public static readonly JsonCollectionsBridge Standard = new JsonCollectionsBridge(
			() => new Dictionary<string, object>(StringComparer.Ordinal),
			d => d,
			l => l);

		public readonly Func<IDictionary<string, object>> CreateDictionary;
		public readonly Func<IDictionary<string, object>, IDictionary<string, object>> WrapDictionary;
		public readonly Func<List<object>, ICollection<object>> WrapJavascriptArrays;


		public object ParseJson(string json) {
			ArgumentValidator.ThrowIfNullOrEmpty(json, "json");

			return ParseJson(new JsonTextReader(new StringReader(json)));
		}

		public object ParseJson(JsonTextReader r) {
			ReadSkippingComments(r);

			if (r.ValueType != null) {
				return r.Value;
			}

			switch (r.TokenType) {
				case JsonToken.StartObject:
					return ParseObject(r);
				case JsonToken.StartArray:
					return ParseArray(r);
                case JsonToken.Null:
				case JsonToken.EndArray:
					return null;
			}

			ThrowOnUnknownToken(r.TokenType);
			return null;
		}

		private object ParseArray(JsonTextReader r) {
			var elements = new List<object>();

			object o;
			while ((o = ParseJson(r)) != null) {
                elements.Add(o);
			}

			return 0 == elements.Count ? new List<object>(0) : new List<object>(elements);
		}

		private static void ThrowOnUnknownToken(JsonToken jsonToken) {
			throw new InvalidOperationException("Unexpected token: " + jsonToken);
		}

		private object ParseObject(JsonTextReader r) {
			var o = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			ReadSkippingComments(r);

			while (r.TokenType != JsonToken.EndObject) {
				switch (r.TokenType) {
					case JsonToken.PropertyName:
						o[(string)r.Value] = ParseJson(r);
						break;
                    default:
						ThrowOnUnknownToken(r.TokenType);
						break;
				}
				ReadSkippingComments(r);
			}

			return this.WrapDictionary(o);
		}

		public static JsonReader ReadSkippingComments(JsonReader r) {
			ArgumentValidator.ThrowIfNull(r, "r");

			do {
				r.Read();
			} while (r.TokenType == JsonToken.Comment);

			return r;
		}

		public object DeepCloneViaJson(object o) {
			ArgumentValidator.ThrowIfNull(o, "o");

			return ParseJson(o.ToJson());
		}

		private static readonly ReadOnlyCollection<object> EmptyList = new ReadOnlyCollection<object>(new List<object>(0));
	}
}
