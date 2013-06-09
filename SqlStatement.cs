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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Helpers
{
    public enum SqlStatementUnionMode
    {
        Distinct, All
    }

    public abstract class SqlStatement
    {
        protected SqlStatement()
        {
            ProviderType = Database.DefaultProvider;
        }

        protected SqlStatement(DatabaseProviderType databaseProvider)
        {
            ProviderType = databaseProvider;
        }

        public DatabaseProviderType ProviderType { get; private set; }

        public abstract string GetUtcDate();
        public abstract string GetDate();
        public abstract string GetUtcOffset();
        public abstract string GetTopStatement(int recordCount);
        public abstract string GetCharIndexFunction(string substring, string stringValue, int startIndex);



        public static string ResolveCrossDatabaseReferences(string sqlStatement)
        {
            return ResolveCrossDatabaseReferences(sqlStatement, Database.DefaultProvider, "EWM_");
        }

        public static string ResolveCrossDatabaseReferences(string sqlStatement, DatabaseProviderType providerType)
        {
            return ResolveCrossDatabaseReferences(sqlStatement, providerType, "EWM_");
        }

        public static string ResolveCrossDatabaseReferences(string sqlStatement, string databaseNamePrefix)
        {
            return ResolveCrossDatabaseReferences(sqlStatement, Database.DefaultProvider, databaseNamePrefix);
        }

        public static string ResolveCrossDatabaseReferences(string sqlStatement, DatabaseProviderType providerType, string databaseNamePrefix)
        {
            const string pattern = @"\bEWM_.*?\.";
            if (providerType == DatabaseProviderType.SqlServer)
            {
                var regEx = new Regex(pattern, RegexOptions.IgnoreCase);
                var results = regEx.Matches(sqlStatement).Cast<Match>().Where(i => i.Success).Select(i => i.Value);
                sqlStatement = results.Aggregate(sqlStatement, (current, item) => current.Replace(item, "{0}.".Fi(item)));
            }
            return sqlStatement;
        }

        public static SqlStatement CreateInstance()
        {
            return CreateInstance(Database.DefaultProvider);
        }

        public static SqlStatement CreateInstance(DatabaseProviderType providerType)
        {
            if (providerType == DatabaseProviderType.MySql) return new MySqlStatement();
            return new SqlServerStatement();
        }

    }
}