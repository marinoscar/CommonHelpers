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
using System.Data;
using System.Xml.Linq;

namespace Common.Helpers
{
    public enum ChangeType { Unknown, Insert, Update };

    public class RowChange : IDisposable
    {
        public RowChange(TableChanges tableChanges)
        {
            ArgumentValidator.ThrowIfNull(tableChanges, "tableChanger");

            this.m_tableChanges = tableChanges;
            this._values = new object[tableChanges.ColumnNames.Length];
            this.ChangeType = ChangeType.Unknown;
        }

        public object this[string columnName]
        {
            get
            {
                var idx = this.m_tableChanges.IdxFromColumnName(columnName);
                return this._values[idx];
            }

            set
            {
                var idx = this.m_tableChanges.IdxFromColumnName(columnName);
                this._values[idx] = value;
            }
        }

        public object this[int idx]
        {
            get { return this._values[idx]; }
            set { this._values[idx] = value; }
        }

        private readonly object[] _values;
        private List<Object> _annotations;
        private readonly TableChanges m_tableChanges;
        public TableChanges TableChanges
        {
            get { return this.m_tableChanges; }
        }

        public ChangeType ChangeType { get; set; }

        public bool IsUnknownChange
        {
            get { return ChangeType.Unknown == this.ChangeType; }
        }

        public object TryGet(string annotationOrColumnName)
        {
            bool success;
            var idx = this.TableChanges.TryGetColumnIdx(annotationOrColumnName, out success);

            if (success)
            {
                return this[idx];
            }

            idx = this.TableChanges.TryGetAnnotationIdx(annotationOrColumnName, out success);
            if (!success)
            {
                return null;
            }

            return this.GetAnnotation(idx);
        }

        public object GetAnnotation(string annotation)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(annotation, "annotation");

            return this.GetAnnotation(this.TableChanges.GetAnnotationIdx(annotation));
        }

        public object GetAnnotation(int idx)
        {
            ArgumentValidator.ThrowIfTrue(idx < 0, "idx must be >= 0 but was {0}".Fi(idx));

            if (null == this._annotations)
            {
                return null;
            }

            if (this._annotations.Count <= idx)
            {
                return null;
            }

            return this._annotations[idx];
        }

        public void SetAnnotation(string name, object value)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(name, "name");

            if (null == this._annotations)
            {
                this._annotations = new List<object>(this.m_tableChanges.Annotations.Count);
            }

            var annotations = this.m_tableChanges.Annotations;
            var idx = -1;

            for (var i = 0; i < annotations.Count; i++)
            {
                if (annotations[i].OicEquals(name))
                {
                    idx = i;
                    break;
                }
            }

            if (-1 == idx)
            {
                this.m_tableChanges.Annotations.Add(name);
                idx = this.m_tableChanges.Annotations.Count - 1;
            }

            while (this._annotations.Count <= idx)
            {
                this._annotations.Add(null);
            }

            this._annotations[idx] = value;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();
            if (_values != null)
            {
                for (var i = 0; i < _values.Length; i++)
                {
                    result[TableChanges.ColumnNames[i]] = _values[i];
                }
            }
            if (_annotations != null)
            {
                for (var i = 0; i < _annotations.Count; i++)
                {
                    result[TableChanges.Annotations[i]] = _annotations[i];
                }
            }
            return result;
        }

        public static readonly Func<string, bool> FilterNone = s => true;
        public static readonly Func<string, bool> FilterStagingColumns = s => !s.OicEquals("UtcLastModifiedOn") && !s.OicEquals("DeletedBy") && !s.OicEquals("Deleted");

        public void ApplyDictionary<T>(Dictionary<string, T> d)
        {
            ApplyDictionary<T>(d, true);
        }

        public void ApplyDictionary<T>(Dictionary<string, T> d, bool storeExtraneousColumnsAsAnnotations)
        {
            ArgumentValidator.ThrowIfNull(d, "d");

            foreach (var pair in d)
            {
                TableColumn c;
                if (!this.m_tableChanges.ColumnsByName.TryGetValue(pair.Key, out c))
                {
                    SetAnnotation(pair.Key, pair.Value);
                    continue;
                }

                if (null == pair.Value)
                {
                    this[pair.Key] = DBNull.Value;
                    continue;
                }

                this[pair.Key] = Convert.ChangeType(pair.Value, c.Type);
            }
        }

        public void ApplyXElement(XElement element)
        {
            ArgumentValidator.ThrowIfNull(element, "element");

            foreach (var xAttribute in element.Attributes())
            {
                var name = xAttribute.Name.ToString();

                TableColumn c;
                if (!this.m_tableChanges.ColumnsByName.TryGetValue(name, out c))
                {
                    continue;
                }


                this[name] = Convert.ChangeType(xAttribute.Value, c.Type);
            }
        }

        public void ApplyObjectProperties(Object o)
        {
            ArgumentValidator.ThrowIfNull(o, "o");

            var t = o.GetType();
            foreach (var column in this.m_tableChanges.Columns)
            {
                var p = ReflectionHelper.TryGetProperty(t, column.Name, ReflectionHelper.InstanceAllInclusiveIgnoreCase);
                if (p != null)
                {
                    this[column.Name] = p.GetValue(o, null);
                }
            }
        }

        public void ApplyDataReader(IDataRecord reader)
        {
            this.ApplyDataReader(reader, true, FilterNone);
        }

        public void ApplyDataReader(IDataRecord reader, bool convertDbNullToDotNetNull, bool storeExtraneousColumnsAsAnnotations)
        {
            Func<string, bool> filter;

            if (storeExtraneousColumnsAsAnnotations)
            {
                filter = s => true;
            }
            else
            {
                filter = s => this.m_tableChanges.ColumnsByName.ContainsKey(s);
            }

            this.ApplyDataReader(reader, convertDbNullToDotNetNull, filter);
        }

        public void ApplyDataReader(IDataRecord reader, bool convertDbNullToDotNetNull, Func<string, bool> columnFilter)
        {
            ArgumentValidator.ThrowIfNull(reader, "reader");
            ArgumentValidator.ThrowIfNull(columnFilter, "columnFilter");

            for (var i = 0; i < reader.FieldCount; i++)
            {
                bool success;

                var columnName = reader.GetName(i);

                if (!columnFilter(columnName))
                {
                    continue;
                }

                var value = reader.GetValue(i);
                if (Convert.IsDBNull(value) && convertDbNullToDotNetNull)
                {
                    value = null;
                }

                var myIdx = this.m_tableChanges.TryGetColumnIdx(columnName, out success);
                if (!success)
                {
                    this.SetAnnotation(reader.GetName(i), value);
                    continue;
                }

                this._values[myIdx] = value;
            }
        }

        public void Dispose()
        {
            this._annotations = null;
        }
    }
}