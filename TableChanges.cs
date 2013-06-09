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
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Common.Helpers
{
    public class TableChanges : IDisposable
    {
        private List<RowChange> m_rowChanges;
        private string m_tableName;

        public bool UseBcpForInserts = true;
        public bool HasBeenPersisted { get; private set; }

        public object GetDefaultValueForColumn(RowChange r, TableColumn c)
        {
            foreach (var g in DefaultColumnValueGenerators)
            {
                var v = g(r, c);

                if (v != null)
                {
                    return v;
                }
            }

            var sb = new StringBuilder(r.TableChanges.Columns.Length * 50);
            foreach (var columnName in r.TableChanges.ColumnNames)
            {
                sb.AppendFormat("{0} = {1}, ", columnName, r[columnName]);
            }

            sb.Length -= 2;

            throw new AssertionViolationException("Cannot generate default value for column {0} of type {1} in table {2}. Row values are: {3}".Fi(
                c.Name, c.Type.Name, r.TableChanges.TableName, sb));
        }

        public string TableName
        {
            get { return m_tableName; }
        }

        public List<string> Annotations { get; private set; }
        public List<Func<RowChange, TableColumn, object>> DefaultColumnValueGenerators = new List<Func<RowChange, TableColumn, object>> {
			(r, c) => c.GenerateDefaultValueOrNull()
		};

        public TableChanges(string tableName, Database database)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(tableName, "tableName");
            ArgumentValidator.ThrowIfNull(database, "database");

            Annotations = new List<string>();
            m_rowCacheByKeys = new Dictionary<object, object>();
            m_tableName = tableName;
            m_rowChanges = new List<RowChange>(32);
            m_database = database;

            InitTableSchema();
        }

        public RowChange NewRowChanges()
        {
            var rowChanges = new RowChange(this);
            RowChanges.Add(rowChanges);
            InitKeys(rowChanges);
            return rowChanges;
        }

        public Action<RowChange> InitKeys = r => { };

        public string[] ColumnNames { get; private set; }
        public TableColumn[] Columns { get; private set; }

        public List<RowChange> RowChanges
        {
            get { return m_rowChanges; }
        }

        public Database Database
        {
            get { return m_database; }
        }

        private void InitTableSchema()
        {
            var query = "SELECT * FROM {0} WHERE 1 = 2".Fi(m_tableName);

            var schemaTable = (DataTable)Database.WithDataReader(query, CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo, r => r.GetSchemaTable());

            var cntColumns = schemaTable.Rows.Count;
            var columns = new List<TableColumn>(cntColumns);
            DataTable = new DataTable(m_tableName);
            ColumnsByName = new Dictionary<string, TableColumn>(cntColumns, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < cntColumns; i++)
            {
                var row = schemaTable.Rows[i];
                var etlColumn = new TableColumn(row, columns.Count);

                DataTable.Columns.Add(etlColumn.Name, etlColumn.Type);

                var isReadOnly = row["IsReadOnly"].Or(false);
                if (isReadOnly)
                {
                    continue;
                }

                columns.Add(etlColumn);
                ColumnsByName[etlColumn.Name] = etlColumn;
            }

            ColumnNames = columns.Select(c => c.Name).ToArray();
            Columns = columns.ToArray();
            LoadKeyNames();
        }

        private void LoadKeyNames()
        {
            KeyNames = new ReadOnlyCollection<string>(Columns.Where(c => c.IsKey).Select(c => c.Name).ToList());
        }

        public void SetColumnsAsKeys(IEnumerable<string> names)
        {
            ArgumentValidator.ThrowIfNull(names, "names");

            foreach (var name in names)
            {
                SetColumnAsKey(name);
            }
        }

        private void SetColumnAsKey(string name)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(name, "name");

            var c = ColumnsByName[name];
            c.IsKey = true;
            c.AllowDbNull = false;
            LoadKeyNames();
        }

        public RowChange GetOrCreateByKeyValues(params object[] keys)
        {
            ArgumentValidator.ThrowIfNull(keys, "keys");

            if (keys.Length != KeyNames.Count)
            {
                throw new AssertionViolationException("You must pass one key value for each key in KeyNames");
            }

            int i;
            var d = m_rowCacheByKeys;
            for (i = 0; i < keys.Length - 1; i++)
            {
                if (!d.ContainsKey(keys[i]))
                {
                    d[keys[i]] = new Dictionary<object, object>();
                }

                d = (Dictionary<object, object>)d[keys[i]];
            }

            if (!d.ContainsKey(keys[i]))
            {
                var r = NewRowChanges();
                for (var j = 0; j < keys.Length; j++)
                {
                    r[KeyNames[j]] = keys[j];
                }
                d[keys[i]] = r;
            }

            return (RowChange)d[keys[i]];
        }

        public int IdxFromColumnName(string columnName)
        {
            bool success;
            var idx = TryGetColumnIdx(columnName, out success);

            if (success)
            {
                return idx;
            }

            throw new ArgumentOutOfRangeException("{0} is not a valid column name for {1}".Fi(columnName, m_tableName));
        }

        public int TryGetColumnIdx(string columnName, out bool success)
        {
            for (var i = 0; i < ColumnNames.Length; i++)
            {
                if (ColumnNames[i].OicEquals(columnName))
                {
                    success = true;
                    return i;
                }
            }

            success = false;
            return -1;
        }

        public void Persist()
        {
            if (0 == RowCount)
            {
                return;
            }

            Upsert();
            HasBeenPersisted = true;
        }

        private void Upsert()
        {
            FigureOutWhetherToInsertOrUpdateEachRow();
            SqlBuilder.Upsert();
        }

        private readonly Database m_database;
        private Dictionary<object, object> m_rowCacheByKeys;
        public Dictionary<string, TableColumn> ColumnsByName;
        private SqlBuilder m_sqlBuilder;
        public DataTable DataTable;

        public int RowCount
        {
            get { return RowChanges.Count; }
        }

        public ReadOnlyCollection<string> KeyNames { get; private set; }

        public void Clear()
        {
            m_rowChanges.Clear();
        }


        public bool HasColumnOrAnnotation(string columnOrAnnotationName)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(columnOrAnnotationName, "columnOrAnnotationName");

            bool success;
            return TryGetColumnIdx(columnOrAnnotationName, out success) != -1 || TryGetAnnotationIdx(columnOrAnnotationName, out success) != -1;
        }

        public int TryGetAnnotationIdx(string annotation, out bool success)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(annotation, "annotation");

            for (int i = 0; i < Annotations.Count; i++)
            {
                if (Annotations[i].OicEquals(annotation))
                {
                    success = true;
                    return i;
                }
            }

            success = false;
            return -1;
        }

        public int GetAnnotationIdx(string annotation)
        {
            bool found;

            var idx = TryGetAnnotationIdx(annotation, out found);
            if (found)
            {
                return idx;
            }

            throw new ArgumentException("Cannot find index for annotation '{0}'".Fi(annotation));
        }

        private void FigureOutWhetherToInsertOrUpdateEachRow()
        {
            if (Database.ProviderType == DatabaseProviderType.MySql) return;
            if (0 < KeyNames.Count)
            {
                SqlBuilder.CheckRowExistenceBasedOnPrimaryKeys();
            }

            foreach (var rowChange in RowChanges)
            {
                if (rowChange.IsUnknownChange)
                {
                    rowChange.ChangeType = ChangeType.Insert;
                }
            }
        }

        public Func<RowChange, T> MakeGetFor<T>(string annotationOrColumnName)
        {
            bool isColumn;
            var idx = TryGetColumnIdx(annotationOrColumnName, out isColumn);
            if (!isColumn)
            {
                idx = GetAnnotationIdx(annotationOrColumnName);
            }

            return rowChange =>
            {
                try
                {
                    var v = isColumn ? rowChange[idx] : rowChange.GetAnnotation(idx);
                    return v == null ? default(T) : (T)v;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException("Cannot convert value {0} in '{1}' to type {2}".Fi(rowChange[idx], annotationOrColumnName, typeof(T).Name));
                }
            };
        }

        private SqlBuilder SqlBuilder
        {
            get
            {
                if (null == m_sqlBuilder)
                {
                    m_sqlBuilder = new SqlBuilder(this);
                }

                return m_sqlBuilder;
            }
        }

        public RowChange Find(Func<RowChange, bool> predicate)
        {
            return RowChanges.FirstOrDefault(predicate);
        }

        public void Dispose()
        {
            m_tableName = null;
            m_sqlBuilder = null;
            m_rowCacheByKeys = null;
            m_rowChanges = null;
        }
    }
}