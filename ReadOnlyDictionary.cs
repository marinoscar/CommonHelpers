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
using System.Collections;
using System.Collections.Generic;
using Common.Helpers;

public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary {
	readonly IDictionary<TKey, TValue> m_dict;

	public ReadOnlyDictionary() {
		this.m_dict = new Dictionary<TKey, TValue>(0);
	}


	public ReadOnlyDictionary(IDictionary<TKey, TValue> backingDict) {
		ArgumentValidator.ThrowIfNull(backingDict, "backingDict");

		this.m_dict = backingDict;
	}

	public void Add(TKey key, TValue value) {
		throw new InvalidOperationException();
	}

	public bool ContainsKey(TKey key) {
		return this.m_dict.ContainsKey(key);
	}

	object IDictionary.this[object key] {
		get { return (this.m_dict as IDictionary)[key]; }
		set { throw new InvalidOperationException(); }
	}

	ICollection IDictionary.Keys {
		get { return (this.m_dict as IDictionary).Keys; }
	}

	ICollection IDictionary.Values {
		get { return (this.m_dict as IDictionary).Values; }
	}

	public ICollection<TKey> Keys {
		get { return this.m_dict.Keys; }
	}

	public bool Remove(TKey key) {
		throw new InvalidOperationException();
	}

	public bool TryGetValue(TKey key, out TValue value) {
		return this.m_dict.TryGetValue(key, out value);
	}

	public ICollection<TValue> Values {
		get { return this.m_dict.Values; }
	}

	public TValue this[TKey key] {
		get { return this.m_dict[key]; }
		set { throw new InvalidOperationException(); }
	}

	public void Add(KeyValuePair<TKey, TValue> item) {
		throw new InvalidOperationException();
	}

	public bool Contains(object key) {
		return (this.m_dict as IDictionary).Contains(key);
	}

	public void Add(object key, object value) {
		(this.m_dict as IDictionary).Add(key, value);
	}

	public void Clear() {
		throw new InvalidOperationException();
	}

	IDictionaryEnumerator IDictionary.GetEnumerator() {
		return (this.m_dict as IDictionary).GetEnumerator();
	}

	public void Remove(object key) {
		throw new InvalidOperationException();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item) {
		return this.m_dict.Contains(item);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
		this.m_dict.CopyTo(array, arrayIndex);
	}

	public void CopyTo(Array array, int index) {
		(this.m_dict as IDictionary).CopyTo(array, index);
	}

	public int Count {
		get { return this.m_dict.Count; }
	}

	public object SyncRoot {
		get { throw new InvalidOperationException(); }
	}

	public bool IsSynchronized {
		get { return true; }
	}

	public bool IsReadOnly {
		get { return true; }
	}

	public bool IsFixedSize {
		get { return true; }
	}

	public bool Remove(KeyValuePair<TKey, TValue> item) {
		throw new InvalidOperationException();
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
		return this.m_dict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return ((IEnumerable)this.m_dict).GetEnumerator();
	}
}