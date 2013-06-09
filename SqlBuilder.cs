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
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Common.Helpers
{
    public class SqlBuilder
    {
        private readonly TableChanges _tableChanges;
        private List<DataRow> _bcpDataRows;
        private DatabaseProviderType _providerType;

        public SqlBuilder(TableChanges tableChanges)
            : this(tableChanges, Database.DefaultProvider)
        {
        }

        public SqlBuilder(TableChanges tableChanges, DatabaseProviderType providerType)
        {
            _tableChanges = tableChanges;
            _builder = new StringBuilder(_tableChanges.RowCount / 5);
            _providerType = providerType;
        }

        public void CheckRowExistenceBasedOnPrimaryKeys()
        {
            var idx = 0;
            while (idx < _tableChanges.RowChanges.Count)
            {
                CheckExistenceOfRows(ref idx);

                _tableChanges.Database.WithDataReader(_builder.ToString(), r =>
                {
                    while (r.Read())
                    {
                        var index = (int)r["i"];
                        _tableChanges.RowChanges[index].ChangeType = ChangeType.Update;
                    }
                    return null;
                });
            }
        }


        public void Upsert()
        {
            if(_providerType == DatabaseProviderType.MySql) MySqlUpsert();
            else SqlServerUpsert();
        }

        private void SqlServerUpsert()
        {
            DoInserts();
            DoUpdates();
        }

        private void MySqlUpsert()
        {
            var idx = 0;
            while (idx < _tableChanges.RowChanges.Count)
            {
                RunMySqlInsert(ref idx);
                _tableChanges.Database.ExecuteNonQuery(_builder.ToString());
            }
        }

        public void DoInserts()
        {
            var idx = 0;
            while (idx < _tableChanges.RowChanges.Count)
            {
                RunInserts(ref idx);
            }
        }

        private void RunInserts(ref int idx)
        {
            _builder.Length = 0;

            AddSetNoCount();

            if (_tableChanges.RowChanges.Count < MinBcpRecordCount || !_tableChanges.UseBcpForInserts)
            {
                RunTempTableInsert(ref idx);
            }
            else
            {
                RunBcpInsert(ref idx);
            }
        }

        private void RunBcpInsert(ref int idx)
        {

            if (_providerType != DatabaseProviderType.SqlServer)
            {
                RunTempTableInsert(ref idx);
                return;
            }

            var insertsExist = false;

            _bcpDataRows = new List<DataRow>();

            for (; idx < _tableChanges.RowChanges.Count; idx++)
            {
                var rowChange = _tableChanges.RowChanges[idx];

                if (rowChange.ChangeType != ChangeType.Insert)
                {
                    continue;
                }

                insertsExist = true;
                AppendRowForBcp(_tableChanges.DataTable, rowChange, c => true);

                if (MaxBcpInsertCount < _bcpDataRows.Count)
                {
                    idx++;
                    break;
                }
            }

            if (insertsExist) _tableChanges.Database.WithConnection(DoSqlServerBulkCopy);
            _bcpDataRows = new List<DataRow>();
        }

        public object DoSqlServerBulkCopy(DbConnection c)
        {
            var sbc = new SqlBulkCopy((SqlConnection)c) { DestinationTableName = _tableChanges.TableName, BulkCopyTimeout = 1800 };
            try
            {
                sbc.WriteToServer(_bcpDataRows.ToArray());
            }
            catch (SqlException)
            {
                var columns = _bcpDataRows[0].Table.Columns;
                var sb = new StringBuilder(columns.Count * 30);

                foreach (var bcpDataRow in _bcpDataRows)
                {
                    foreach (DataColumn col in columns)
                    {
                        sb.AppendFormat("{0} = {1}   ", col.ColumnName, bcpDataRow[col]);
                    }

                    // SHOULD: report this more appropriately
                    Console.WriteLine(sb);
                    sb.Length = 0;
                }
                throw;
            }

            _bcpDataRows = null;
            return null;
        }


        private void RunMySqlInsert(ref int idx)
        {
            _builder.Clear();
            _builder.AppendFormat("INSERT INTO {0} ({1}) VALUES", 
                _tableChanges.TableName,
                string.Join(",", _tableChanges.ColumnNames.Select(c => c.FormatSqlColumnName())));
            for (; idx < _tableChanges.RowChanges.Count; idx++)
            {
                _builder.Append("\n(");
                AddColumnValues(_tableChanges.RowChanges[idx], c => true);
                _builder.AppendLine("),");
                if (MaxSqlStatementLength < _builder.Length)
                {
                    idx++;
                    break;
                }
            }
            _builder.Length -= 3;
            _builder.Append("\nON DUPLICATE KEY UPDATE ");
            foreach (var columnName in _tableChanges.ColumnNames)
            {
                RunMySqlOnDuplicateKeyUpdateValues(columnName);
            }
            _builder.Length -= 1;
            _builder.AppendLine(";");
        }

        private void RunMySqlOnDuplicateKeyUpdateValues(string columnName)
        {
            _builder.AppendFormat("{0}=VALUES({0}),", columnName.FormatSqlColumnName());
        }

        private void RunTempTableInsert(ref int idx)
        {
            bool insertsExist = false;

            AddTableVariable(false, c => true);

            for (; idx < _tableChanges.RowChanges.Count; idx++)
            {
                var rowChange = _tableChanges.RowChanges[idx];

                if (rowChange.ChangeType != ChangeType.Insert)
                {
                    continue;
                }

                insertsExist = true;
                AppendInsertIntoTableVariable(rowChange, c => true);

                if (MaxSqlStatementLength < _builder.Length)
                {
                    idx++;
                    break;
                }
            }

            _builder.AppendLine();
            _builder.AppendFormat("INSERT {0} SELECT * FROM {1};", _tableChanges.TableName, GetTableVariableName());

            if (insertsExist) _tableChanges.Database.ExecuteNonQuery(_builder.ToString());
            _builder.Length = 0;
        }

        public void DoUpdates()
        {
            _builder.Length = 0;
            var skippedRows = _tableChanges.RowChanges.Where(r => r.ChangeType == ChangeType.Update).ToList();

            while (0 < skippedRows.Count)
            {
                var rowsToDo = skippedRows;
                skippedRows = new List<RowChange>();

                InstallUpdateColumnMap(rowsToDo[0]);

                foreach (var rowChange in rowsToDo)
                {
                    if (!MatchesUpdateColumnMap(rowChange))
                    {
                        skippedRows.Add(rowChange);
                        continue;
                    }

                    QueueRowForUpdate(rowChange);
                }

                RunUpdateQuery();
            }
        }

        private void AppendRowForBcp(DataTable tableSchema, RowChange rowChange, Func<TableColumn, bool> filter)
        {
            var row = tableSchema.NewRow();

            for (var i = 0; i < _tableChanges.ColumnNames.Length; i++)
            {
                var columnName = _tableChanges.ColumnNames[i];

                var column = _tableChanges.ColumnsByName[columnName];
                if (!filter(column))
                {
                    continue;
                }

                if (null == rowChange[i] && column.IsKey)
                {
                    continue;
                }

                var columnValue = rowChange[i] ?? _tableChanges.GetDefaultValueForColumn(rowChange, column) ?? DBNull.Value;

                row[columnName] = columnValue;
            }

            _bcpDataRows.Add(row);
        }

        private void AppendInsertIntoTableVariable(RowChange rowChange, Func<TableColumn, bool> filter)
        {
            _builder.AppendLine();
            _builder.Append("INSERT {0} VALUES (".FormatInvariant(GetTableVariableName()));
            AddColumnValues(rowChange, filter);
            _builder.Append(")");
            if (_providerType == DatabaseProviderType.MySql) _builder.Append(";");
        }

        private bool MatchesUpdateColumnMap(RowChange rowChange)
        {
            for (var i = 0; i < _columnMap.Length; i++)
            {
                if (_columnMap[i] != IsUpdateColumn(rowChange, i))
                {
                    return false;
                }
            }

            return true;
        }

        private void InstallUpdateColumnMap(RowChange rowChange)
        {
            var cntColumns = _tableChanges.ColumnNames.Length;

            if (null == _columnMap)
            {
                _columnMap = new bool[cntColumns];
            }

            _hasUpdateColumns = false;

            for (var i = 0; i < cntColumns; i++)
            {
                _columnMap[i] = IsUpdateColumn(rowChange, i);
                _hasUpdateColumns |= _columnMap[i];
            }
        }

        private bool IsUpdateColumn(RowChange rowChange, int i)
        {
            return !_tableChanges.Columns[i].IsKey;
        }

        private void QueueRowForUpdate(RowChange rowChange)
        {
            if (!_hasUpdateColumns)
            {
                return;
            }

            Func<TableColumn, bool> filter = c => _columnMap[c.Index] || c.IsKey;

            if (0 == _builder.Length)
            {
                AddSetNoCount();
                AddTableVariable(false, filter);
            }

            AppendInsertIntoTableVariable(rowChange, filter);

            if (_builder.Length < MaxSqlStatementLength)
            {
                return;
            }

            RunUpdateQuery();
        }

        private void RunUpdateQuery()
        {
            if (0 == _builder.Length)
            {
                return;
            }

            GetUpdateStatement();

            _builder.AppendFormat("\nWHERE\n");

            for (var i = 0; i < _tableChanges.ColumnNames.Length; i++)
            {
                var c = _tableChanges.Columns[i];
                if (!_columnMap[i] || c.IsKey || c.IsTimestamp)
                {
                    continue;
                }

                var handleNulls = string.Empty;
                if (c.AllowDbNull)
                {
                    handleNulls = "OR ({0}.{1} IS NULL AND {2}.{1} IS NOT NULL) OR ({0}.{1} IS NOT NULL AND {2}.{1} IS NULL)".Fi(
                        _tableChanges.TableName, _tableChanges.ColumnNames[i], GetTableVariableName().FormatSqlColumnName());
                }

                _builder.AppendFormat("( ({0}.{1} <> {3}.{1}) {2} ) OR ", _tableChanges.TableName, _tableChanges.ColumnNames[i], handleNulls, GetTableVariableName().FormatSqlColumnName());
            }

            _builder.Length -= 3;
            if (_providerType == DatabaseProviderType.MySql) _builder.Append(";");
            _tableChanges.Database.ExecuteNonQuery(_builder.ToString());
            _builder.Length = 0;
        }


        private void GetUpdateStatement()
        {
            if (_providerType == DatabaseProviderType.SqlServer)
                GetSqlServerUpdate();
            else
                GetMySqlUpdate();
        }

        private void GetMySqlUpdate()
        {
            var tableName = _tableChanges.TableName;

            _builder.AppendLine();
            _builder.AppendFormat("\nUPDATE {0}", tableName);
            _builder.AppendLine();
            _builder.AppendFormat("INNER JOIN {0} ON (", GetTableVariableName());
            foreach (var keyName in _tableChanges.KeyNames)
            {
                _builder.AppendFormat("{0}.{1} = {2}.{1} AND ", tableName, keyName, GetTableVariableName());
            }

            _builder.Length -= 4;
            _builder.AppendLine(")");
            _builder.AppendLine("SET");

            for (int i = 0; i < _tableChanges.ColumnNames.Length; i++)
            {
                var col = _tableChanges.Columns[i];

                if (col.IsTimestamp)
                {
                    _builder.AppendFormat("{0}.{1} = {2}, ", tableName, col.Name, _tableChanges.GetDefaultValueForColumn(null, col).ToSql());
                    continue;
                }

                if (!_columnMap[i] || col.IsKey)
                {
                    continue;
                }

                _builder.AppendFormat("{0}.{1} = {2}.{1}, ", tableName, col.Name, GetTableVariableName());
            }

            _builder.Length -= 2;
        }


        private void GetSqlServerUpdate()
        {
            var tableName = _tableChanges.TableName;

            _builder.AppendLine();
            _builder.AppendFormat("UPDATE {0} SET", tableName);
            _builder.AppendLine();

            for (int i = 0; i < _tableChanges.ColumnNames.Length; i++)
            {
                var col = _tableChanges.Columns[i];

                if (col.IsTimestamp)
                {
                    _builder.AppendFormat("{0}.{1} = {2}, ", tableName, col.Name, _tableChanges.GetDefaultValueForColumn(null, col).ToSql());
                    continue;
                }

                if (!_columnMap[i] || col.IsKey)
                {
                    continue;
                }

                _builder.AppendFormat("{0}.{1} = {2}.{1}, ", tableName, col.Name, GetTableVariableName().FormatSqlColumnName());
            }

            _builder.Length -= 2;

            _builder.AppendLine();
            _builder.AppendFormat("FROM {0} INNER JOIN {1} ON (", tableName, GetTableVariableName());

            foreach (var keyName in _tableChanges.KeyNames)
            {
                _builder.AppendFormat("{0}.{1} = {2}.{1} AND", tableName, keyName, GetTableVariableName().FormatSqlColumnName());
            }

            _builder.Length -= 3;
            _builder.AppendLine(")");
        }

        private void CheckExistenceOfRows(ref int idx)
        {
            _builder.Length = 0;

            AddSetNoCount();
            AddTableVariable(true, c => c.IsKey);

            for (; idx < _tableChanges.RowChanges.Count; idx++)
            {
                var rowChange = _tableChanges.RowChanges[idx];

                if (!rowChange.IsUnknownChange)
                {
                    continue;
                }

                foreach (var keyName in _tableChanges.KeyNames)
                {
                    if (null == rowChange[keyName])
                    {
                        continue;
                    }
                }

                AddRowChangeToTableVariable(rowChange, idx);
                if (MaxSqlStatementLength < _builder.Length)
                {
                    idx++;
                    break;
                }
            }

            _builder.AppendFormat("\nSELECT i FROM {1} INNER JOIN {0} ON ", _tableChanges.TableName, GetTableVariableName());
            foreach (var keyName in _tableChanges.KeyNames)
            {
                _builder.AppendFormat("\n{2}.{0} = {1}.{0} AND", keyName, _tableChanges.TableName, GetTableVariableName().FormatSqlColumnName());
            }
            _builder.Length -= 3;
            if (_providerType == DatabaseProviderType.MySql) _builder.Append(";");
        }

        private void AddSetNoCount()
        {
            if (Database.DefaultProvider == DatabaseProviderType.SqlServer)
                _builder.AppendLine("SET NOCOUNT ON");
        }

        public const int MaxSqlStatementLength = 32700;
        public const int MaxBcpInsertCount = 32700;
        public const int MinBcpRecordCount = 10;

        private string GetTableVariableName()
        {
            return _providerType == DatabaseProviderType.SqlServer
                       ? "@T"
                       : "Tmp";
        }

        private void AddRowChangeToTableVariable(RowChange rowChange, int rowChangeIndex)
        {
            _builder.AppendLine();
            _builder.Append("INSERT {0} VALUES (".Fi(GetTableVariableName()));
            AddColumnValues(rowChange, c => c.IsKey);
            _builder.AppendFormat(", {0})", rowChangeIndex);
            if (_providerType == DatabaseProviderType.MySql) _builder.Append(";");
        }

        private void AddColumnValues(RowChange rowChange, Func<TableColumn, bool> filter)
        {
            for (int i = 0; i < _tableChanges.ColumnNames.Length; i++)
            {
                var columnName = _tableChanges.ColumnNames[i];

                var column = _tableChanges.ColumnsByName[columnName];
                if (!filter(column))
                {
                    continue;
                }

                if (null == rowChange[i] && column.IsKey)
                {
                    continue;
                }

                var columnValue = rowChange[i] ?? _tableChanges.GetDefaultValueForColumn(rowChange, column) ?? DBNull.Value;

                _builder.AppendFormat("{0}, ", columnValue.ToSql());
            }

            _builder.Length -= 2;
        }

        private void AddKeyColumnNames(bool addParenthesis)
        {
            if (addParenthesis)
            {
                _builder.Append("(");
            }

            foreach (var keyName in _tableChanges.KeyNames)
            {
                _builder.AppendFormat("{0}, ", keyName.FormatSqlColumnName());
            }
            _builder.Length -= 2;

            if (addParenthesis)
            {
                _builder.Append(")");
            }
        }

        private void AddTableVariable(bool addIndexForRowChange, Func<TableColumn, bool> filter)
        {
            _builder.AppendLine(_providerType == DatabaseProviderType.SqlServer
                                         ? "DECLARE {0} table (".FormatInvariant(GetTableVariableName())
                                         : "DROP TABLE IF EXISTS {0};\n\nCREATE TEMPORARY TABLE {0} (".FormatInvariant(GetTableVariableName()));

            // add primary key
            _builder.Append("PRIMARY KEY ");
            AddKeyColumnNames(true);
            _builder.AppendLine(",");

            // add column definitions
            foreach (var columnName in _tableChanges.ColumnNames)
            {
                var column = _tableChanges.ColumnsByName[columnName];
                if (!filter(column))
                {
                    continue;
                }

                _builder.AppendFormat("{0} {1} {2} NULL, ", columnName.FormatSqlColumnName(), column.FullSqlDefinition, column.AllowDbNull ? "" : "NOT");
            }
            if (addIndexForRowChange)
            {
                _builder.Append("i int NOT NULL");
            }
            else
            {
                _builder.Length -= 2;
            }

            _builder.Append(")");
            if (_providerType == DatabaseProviderType.MySql) _builder.Append(";");
            _builder.AppendLine();
        }

        private readonly StringBuilder _builder;
        private bool[] _columnMap;
        private bool _hasUpdateColumns;
    }
}