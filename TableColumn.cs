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
using System.Data;
using System.Data.SqlTypes;

namespace Common.Helpers
{
    public class TableColumn
    {
        public TableColumn(DataRow row, int index)
        {
            Index = index;

            Name = (string)row["ColumnName"];
            // MUST: differentiate between primary keys and unique keys, which this doesn't do.
            IsKey = (bool)row["IsKey"];
            Type = (Type)row["DataType"];
            AllowDbNull = (bool)row["AllowDBNull"];
            if (!DBNull.Value.Equals(row["IsIdentity"]))
                IsIdentity = (bool)row["IsIdentity"];

            var size = (int)row["ColumnSize"];
            FullSqlDefinition = GetFullDefinition(size);
        }

        private string GetFullDefinition(int size)
        {
            var typeCode = Type.GetTypeCode(Type);
            var sqltype = "varchar";
            var sqlSize = "({0})".FormatInvariant(size);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    sqltype = "bit";
                    break;
                case TypeCode.Char:
                    sqltype = "varchar";
                    break;
                case TypeCode.DateTime:
                    sqltype = "datetime";
                    break;
                case TypeCode.Decimal:
                    sqltype = "decimal";
                    break;
                case TypeCode.Double:
                    sqltype = "float";
                    break;
                case TypeCode.Int16:
                    sqltype = "smallint";
                    break;
                case TypeCode.Int32:
                    sqltype = "int";
                    break;
                case TypeCode.Int64:
                    sqltype = "bigint";
                    break;
                case TypeCode.Single:
                    sqltype = "float";
                    break;
                case TypeCode.String:
                    sqltype = "varchar";
                    break;
                case TypeCode.UInt64:
                    sqltype = "bit";
                    break;
            }
            return
                (typeCode == TypeCode.Char || typeCode == TypeCode.String || typeCode == TypeCode.Byte)
                    ? "{0} {1}".FormatInvariant(sqltype, sqlSize)
                    : "{0}".FormatInvariant(sqltype);
        }

        public bool IsTimestamp
        {
            get { return Name.OicEquals("UtcLastModifiedOn"); }
        }

        public object GenerateDefaultValueOrNull()
        {
            if (AllowDbNull)
            {
                return DBNull.Value;
            }

            if (ReflectionHelper.IsBool(Type))
            {
                return false;
            }

            if (ReflectionHelper.IsNumeric(Type))
            {
                return Convert.ChangeType(0, Type);
            }

            if (typeof(DateTime) == Type)
            {
                return (DateTime)SqlDateTime.MinValue;
            }

            return null;
        }

        public string Name;
        public bool IsKey;
        public bool AllowDbNull;
        public Type Type;
        public bool IsIdentity;
        public string ColumnSize;
        public string FullSqlDefinition;
        public int Index;
    }
}