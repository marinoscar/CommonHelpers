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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace Common.Helpers
{

    public enum DatabaseProviderType { None, SqlServer, MySql, Postgresql, Oracle, Db2, SqLite }

    public class Database
    {

        #region Variable Declaration

        private readonly string _userName;
        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly string _connectionString;
        private readonly ITransactionResolver _transactionResolver;
        private DbProviderFactory _providerFactory;
        private SqlStatement _sqlStatement;

        private static readonly Regex rgxGo = new Regex(@"^  \s* GO \s*  $", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        #endregion
        
        #region Constructors

        public Database(string connectionString) : this(connectionString, DatabaseProviderType.None, null, new NullTransactionResolver()) { }

        public Database(string connectionString, DatabaseProviderType providerType) : this(connectionString, providerType, null, new NullTransactionResolver()) { }

        public Database(string connectionString, string appNameIfNotSpecified) : this(connectionString, DatabaseProviderType.None, appNameIfNotSpecified, new NullTransactionResolver()) { }

        public Database(string connectionString, string defaultAppName, ITransactionResolver transactionResolver) : this(connectionString, DatabaseProviderType.None, defaultAppName, transactionResolver) { }

        public Database(LocalContextTransactionResolver transactionResolver)
            : this(
                transactionResolver.Connection.ConnectionString, transactionResolver.ProviderType, "",
                transactionResolver)
        {
        }

        public Database(string connectionString, DatabaseProviderType providerType, string defaultAppName, ITransactionResolver transactionResolver)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionString, "connectionString");
            ArgumentValidator.ThrowIfNull(transactionResolver, "transactionResolver");

            if (providerType == DatabaseProviderType.None && DefaultProvider == DatabaseProviderType.None)
                providerType = DatabaseProviderType.SqlServer;
            if (providerType == DatabaseProviderType.None && DefaultProvider != DatabaseProviderType.None)
                providerType = DefaultProvider;
            ProviderType = providerType;

            var connString = new DbConnectionStringBuilder { ConnectionString = connectionString };

            _userName = GetUserIdFromConnStringObject(connString);
            _serverName = GetServerFromConnStringObject(connString);
            _databaseName = GetDatabaseFromConnStringObject(connString);
            _connectionString = connectionString;

            CommandTimeoutInSeconds = 30;
            _transactionResolver = transactionResolver;
            _sqlStatement = SqlStatement.CreateInstance(ProviderType);
        }

        #endregion

        #region Static Methods

        public static bool DoesDbExist(string connectionStringToDb)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionStringToDb, "connectionStringToDb");

            var masterDb = ConnectToMasterDb(connectionStringToDb);
            var dbName = DbNameFromConnectionString(connectionStringToDb);

            bool exists;
            masterDb.TryExecuteScalar<Int16>("SELECT DB_ID({0})".FormatSql(dbName), out exists);
            return exists;
        }

        public static string DbNameFromConnectionString(string connectionString)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionString, "connectionString");

            // MUST: throw exception if connection string does not specify a database
            return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        }

        public static void DropAndCreateDatabase(string connectionStringToServer)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionStringToServer, "connectionStringToServer");

            DropDatabase(connectionStringToServer);
            CreateDatabase(connectionStringToServer);
        }

        public static void CreateDatabase(string connectionStringToDb)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionStringToDb, "connectionStringToDb");
            var databaseName = DbNameFromConnectionString(connectionStringToDb);
            ExecuteStatementInMasterDatabase(connectionStringToDb, "IF (DB_ID({0}) IS NULL) CREATE DATABASE {0:name}".FormatSql(databaseName));
        }

        public static void DropDatabase(string connectionStringToDb)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionStringToDb, "connectionStringToDb");
            var databaseName = DbNameFromConnectionString(connectionStringToDb);

            var masterDb = ConnectToMasterDb(connectionStringToDb);
            var activeConnections = masterDb.ExecuteToList<short>("SELECT SPId FROM MASTER..SysProcesses WHERE DBId = DB_ID({0}) AND cmd <> 'CHECKPOINT'".FormatSql(databaseName));

            foreach (var id in activeConnections)
            {
                masterDb.ExecuteNonQuery("KILL {0}".FormatSql(id));
            }

            masterDb.ExecuteNonQuery("IF (DB_ID({0}) IS NOT NULL) DROP DATABASE {0:name}".FormatSql(databaseName));
        }

        public static Database ConnectToMasterDb(string connectionStringToServer)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionStringToServer, "connectionStringToServer");

            var b = new SqlConnectionStringBuilder(connectionStringToServer) { InitialCatalog = "master" };
            var masterDb = new Database(b.ToString(), typeof(Database).FullName);
            return masterDb;
        }

        public static void ExecuteStatementInMasterDatabase(string connectionStringToServer, string sqlStatement)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionStringToServer, "connectionStringToServer");
            ArgumentValidator.ThrowIfNullOrEmpty(sqlStatement, "sqlStatement");

            ConnectToMasterDb(connectionStringToServer).ExecuteNonQuery(sqlStatement);
        }

        public static void ThrowIfConnectionStringLacksAppName(string connectionString)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionString, "connectionString");

            var b = new SqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrEmpty(b.ApplicationName))
            {
                throw new ArgumentException("You have not specified an ApplicationName in your connection string. This makes it very "
                    + "hard to profile and troubleshoot SQL Server, because connections appear as a generic '.NET Application' instead "
                    + "of your actual app. Please add an 'Application Name=YourAppName;' field to your connection string.", "connectionString");
            }
        }

        public static void SetDefaultDatabaseProviderType(DatabaseProviderType type)
        {
            _dbProvider = type;
        }

        private static DatabaseProviderType _dbProvider;

        public static DatabaseProviderType DefaultProvider
        {
            get
            {
                if (_dbProvider == DatabaseProviderType.None)
                    _dbProvider = DatabaseProviderType.SqlServer;
                return _dbProvider;
            }
        }

        private static string GetUserIdFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("User Id")) return Convert.ToString(connString["User Id"]);
            if (connString.ContainsKey("Uid")) return Convert.ToString(connString["Uid"]);
            return string.Empty;
        }

        private static string GetServerFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("Server")) return Convert.ToString(connString["Server"]);
            if (connString.ContainsKey("Data Source")) return Convert.ToString(connString["Data Source"]);
            if (connString.ContainsKey("Host")) return Convert.ToString(connString["Host"]);
            return string.Empty;
        }

        private static string GetDatabaseFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("Database")) return Convert.ToString(connString["Database"]);
            if (connString.ContainsKey("Initial Catalog")) return Convert.ToString(connString["Initial Catalog"]);
            if (connString.ContainsKey("Host")) return Convert.ToString(connString["Host"]);
            return string.Empty;
        }

        public static string GetConnectionStringFromAspNetConfiguration(string connectionName, HttpContext context)
        {
            var collection = WebConfigurationManager.ConnectionStrings;
            if (null == collection || null == collection[connectionName])
            {
                var msg = ("Unable to find a configured connection string with name '{0}' in web application '{1}'."
                           + "You must add a connection string named {0} to XML element <connectionStrings> in the web.config file for this application.")
                           .Fi(connectionName, context.Request.ApplicationPath);

                throw new BadConfigurationException(msg);
            }

            var connectionString = collection[connectionName].ConnectionString;
            var sqlAppName = "'{0}' sql connection by website rooted at: {1}".Fi(connectionName, HttpContext.Current.Request.ApplicationPath);

            return AddApplicationNameToConnectionString(connectionString, sqlAppName);
        }

        public static string AddApplicationNameToConnectionString(string connectionString, string applicationName)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(connectionString, "connectionString");
            ArgumentValidator.ThrowIfNullOrEmpty(applicationName, "applicationName");

            var b = new SqlConnectionStringBuilder(connectionString) { ApplicationName = applicationName };

            return b.ToString();
        }

        public static DbProviderFactory GetDefaultFactory()
        {
            return DbProviderFactories.GetFactory(GetDatabaseProviderName(DefaultProvider));
        }

        public static DbProviderFactory GetFactoryFromProvider(DatabaseProviderType provider)
        {
            return DbProviderFactories.GetFactory(GetDatabaseProviderName(provider));
        }

        private static string GetDatabaseProviderName(DatabaseProviderType providerType)
        {
            var result = "System.Data.SqlClient";
            switch (providerType)
            {
                case DatabaseProviderType.MySql:
                    result = "MySql.Data.MySqlClient";
                    break;
                case DatabaseProviderType.Postgresql:
                    result = "Npgsql";
                    break;
            }
            return result;
        }

        public static DatabaseProviderType GetProviderTypeFromName(string providerName)
        {
            var result = DatabaseProviderType.SqlServer;
            switch (providerName.ToLowerInvariant())
            {
                case "mysql.data.mysqlclient":
                    result = DatabaseProviderType.MySql;
                    break;
                case "Npgsql":
                    result = DatabaseProviderType.Postgresql;
                    break;
            }
            return result;
        }

        #endregion

        #region Property Implementation

        public DatabaseProviderType ProviderType { get; private set; }

        public DateTime Now
        {
            get { return ExecuteScalar<DateTime>(_sqlStatement.GetDate()); }
        }

        public DateTime UtcNow
        {
            get { return ExecuteScalar<DateTime>(_sqlStatement.GetUtcDate()); }
        }

        public TimeSpan UtcOffset
        {
            get { return TimeSpan.FromMinutes(ExecuteScalar<int>(_sqlStatement.GetUtcOffset())); }
        }

        public string Name
        {
            get { return _databaseName; }
        }

        public string ServerName
        {
            get { return _serverName; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public int CommandTimeoutInSeconds { get; set; }

        public bool Exists
        {
            get { return DoesDbExist(ConnectionString); }
        }

        #endregion
        
        #region Methods

        public Database Copy()
        {
            return new Database(ConnectionString);
        }

        public T ExecuteScalar<T>(string query)
        {
            return (T)WithCommand(query, command =>
            {
                var result = command.ExecuteScalar();
                if (Convert.DBNull == result)
                {
                    throw new InvalidCastException(("Attempt to execute sql query '{0}' and obtain a scalar of type '{1}' "
                        + "returned an unexpected NULL from the database.").Fi(query, typeof(T).FullName));
                }

                if (null == result)
                {
                    throw new InvalidCastException(("Attempt to execute sql query '{0}' and obtain a scalar of type '{1}' "
                        + "returned zero rows.").Fi(query, typeof(T).FullName));
                }

                result = Convert.ChangeType(result, typeof(T));
                return result;
            });
        }

        public T TryExecuteScalar<T>(string query, out bool gotData)
        {
            var closureGotData = false;
            var result = (T)WithCommand(query, command =>
            {
                var returnedValue = command.ExecuteScalar();
                if (Convert.IsDBNull(returnedValue) || null == returnedValue)
                {
                    return default(T);
                }

                closureGotData = true;
                return returnedValue;
            });

            gotData = closureGotData;
            return result;
        }

        public T ExecuteScalarOr<T>(string query, T defaultOnFailure)
        {
            bool success;
            var result = TryExecuteScalar<T>(query, out success);
            return success ? result : defaultOnFailure;
        }

        public void WhileReading(string query, Action<DbDataReader> doSomething)
        {
            WithDataReader(query, CommandBehavior.Default, r =>
            {
                while (r.Read())
                {
                    doSomething(r);
                }

                return null;
            });
        }

        public object WithDataReader(string query, Func<DbDataReader, object> doSomething)
        {
            return WithDataReader(query, CommandBehavior.Default, doSomething);
        }

        public object WithDataReader(string query, CommandBehavior behavior, Func<DbDataReader, object> doSomething)
        {
            return WithCommand(query, command =>
            {
                using (var r = command.ExecuteReader(behavior))
                {
                    return doSomething(r);
                }
            });
        }

        public List<T> ExecuteToList<T>(string query)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(query, "query");

            return (List<T>)WithDataReader(query, r =>
            {
                var list = new List<T>();

                while (r.Read())
                {
                    list.Add((T)r[0]);
                }

                return list;
            });
        }

        public List<T> ExecuteToEntityList<T>(string query)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(query, "query");
            var list = new List<T>();
            WhileReading(query, r =>
            {
                var item = (T)ReflectionHelper.CreateInstance(typeof(T));
                ReflectionHelper.ApplyDataRecordToObject(r, item);
                list.Add(item);
            });
            return list;
        }

        public object WithConnection(Func<DbConnection, object> doSomething)
        {
            ArgumentValidator.ThrowIfNull(doSomething, "doSomething");

            var ambientConnection = _transactionResolver.GetConnectionOrNull();
            if (ambientConnection != null)
            {
                if (ambientConnection.State == ConnectionState.Closed) ambientConnection.Open();
                return doSomething(ambientConnection);
            }

            using (var conn = OpenConnection())
            {
                return doSomething(conn);
            }
        }

        public object WithCommand(string sqlStatement, Func<DbCommand, object> doSomething)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(sqlStatement, "sqlStatement");
            ArgumentValidator.ThrowIfNull(doSomething, "doSomething");

            return WithConnection(conn =>
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlStatement;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.Transaction = _transactionResolver.GetTransactionOrNull();
                cmd.CommandTimeout = CommandTimeoutInSeconds;
                try
                {
                    return doSomething(cmd);
                }
                catch (SqlException ex)
                {
                    throw new DatabaseException("Error running statement:\n{0}\n{1}\n\n with user {2}".Fi(sqlStatement, ex.Message, _userName), ex);
                }
            });
        }

        public DataTable ExecuteToDataTable(string query)
        {
            return (DataTable)WithCommand(query, command =>
            {

                var adapter = GetDefaultFactory().CreateDataAdapter();
                adapter.SelectCommand = command;
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            });
        }

        public SqlBatchResults ExecuteSqlBatch(string sqlBatch)
        {
            return (SqlBatchResults)WithCommand(sqlBatch, command => new SqlBatchResults((SqlCommand)command));
        }

        public bool DoesObjectExist(string dbObjectName)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(dbObjectName, "dbObjectName");

            bool exists;
            TryExecuteScalar<Int32>("SELECT OBJECT_ID({0})".FormatSql(dbObjectName), out exists);

            return exists;
        }

        public int ExecuteNonQuery(string sqlStatement)
        {
            return (int)WithCommand(sqlStatement, command => command.ExecuteNonQuery());
        }

        public void TestConnection()
        {
            WithConnection(conn => conn);
        }

        public void RunSqlScript(FileInfo fileWithSqlScript, params Func<string, string>[] lineFilters)
        {
            ArgumentValidator.ThrowIfDoesNotExist(fileWithSqlScript, "fileWithSqlScript");

            using (var sr = fileWithSqlScript.OpenText())
            {
                var sb = new StringBuilder(512);
                while (!sr.EndOfStream)
                {
                    var nextLine = sr.ReadLine();

                    for (var i = 0; i < lineFilters.Length; i++)
                    {
                        nextLine = lineFilters[i](nextLine);
                    }

                    if (rgxGo.IsMatch(nextLine))
                    {
                        if (0 < sb.Length)
                        {
                            ExecuteNonQuery(sb.ToString());
                        }

                        sb.Length = 0;
                    }
                    else
                    {
                        sb.AppendLine(nextLine);
                    }
                }
                if (0 < sb.Length)
                {
                    ExecuteNonQuery(sb.ToString());
                }
            }
        }

        //public void WithStreamForBlob(string tableName, string columnName, string whereClauseForRow, Action<Stream> doSomething)
        //{
        //    ArgumentValidator.ThrowIfNullOrEmpty(tableName, "tableName");
        //    ArgumentValidator.ThrowIfNullOrEmpty(columnName, "columnName");
        //    ArgumentValidator.ThrowIfNullOrEmpty(whereClauseForRow, "whereClauseForRow");
        //    ArgumentValidator.ThrowIfNull(doSomething, "doSomething");

        //    WithConnection(c =>
        //    {
        //        var dbStream = new VarBinaryStream(new VarBinaryHelper(c, tableName, columnName, whereClauseForRow));
        //        using (var stream = new BufferedStream(dbStream, 16384))
        //        {
        //            doSomething(stream);
        //        }

        //        return null;
        //    });
        //}

        private DbConnection OpenConnection()
        {
            var conn = GetConnection(ConnectionString);
            conn.Open();
            return conn;
        }


        private DbProviderFactory GetFactory()
        {
            return GetFactory(ProviderType);
        }

        private DbProviderFactory GetFactory(DatabaseProviderType provider)
        {
            if (_providerFactory == null) _providerFactory = DbProviderFactories.GetFactory(GetDatabaseProviderName(provider));
            return _providerFactory;
        }

        public DbConnection GetConnection()
        {
            return GetConnection(ProviderType);
        }

        public DbConnection GetConnection(DatabaseProviderType providerType)
        {
            var conn = GetFactoryFromProvider(providerType).CreateConnection();
            conn.ConnectionString = ConnectionString;
            return conn;
        }

        private DbConnection GetConnection(string connString)
        {
            var conn = GetFactory().CreateConnection();
            conn.ConnectionString = connString;
            return conn;
        }



        public T RetrieveOrInsert<T>(string tableName, string columnToRetrieve, T defaultValue, string[] keyNames, object[] keyValues)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(tableName, "tableName");
            ArgumentValidator.ThrowIfNullOrEmpty(columnToRetrieve, "columnToRetrieve");
            ArgumentValidator.ThrowIfNull(keyNames, "keyNames");
            ArgumentValidator.ThrowIfNull(keyValues, "keyValues");

            if (keyNames.Length != keyValues.Length)
            {
                throw new ArgumentException(("The length for the keyNames array is {0}, which is different than the "
                    + "length for the keyValues array of {1}. The arrays must have the same length: each value corresponds "
                    + "to a given key name.").Fi(keyNames.Length, keyValues.Length));
            }

            var queryBuilder = new StringBuilder(128);

            queryBuilder.AppendFormat("SELECT {{0}} FROM {0} WHERE ", tableName);

            var sqlKeyValues = new string[keyValues.Length];

            for (var i = 0; i < keyNames.Length; i++)
            {
                sqlKeyValues[i] = keyValues[i].ToSql();
                queryBuilder.AppendFormat("{0} = {1} AND ", keyNames[i], sqlKeyValues[i]);
            }

            queryBuilder.Length -= 4;

            var selectQuery = queryBuilder.ToString();

            var cntFound = ExecuteScalar<int>(selectQuery.Fi("COUNT(1)"));
            if (0 == cntFound)
            {
                queryBuilder.Length = 0;

                queryBuilder.AppendFormat("INSERT {0} ({1},{2}) VALUES ({3},{4})", tableName, keyNames.Join(","), columnToRetrieve,
                    sqlKeyValues.Join(","), defaultValue.ToSql());

                ExecuteNonQuery(queryBuilder.ToString());
                // we could return here and avoid the query to the database, but I'd rather go ahead and re-select the value
                // we just inserted to make sure everything always works the same (insert is successful, there are no
                // conversion errors, the return value for the first call is guaranteed to be identical to the second, etc.)
            }

            return ExecuteScalar<T>(selectQuery.Fi(columnToRetrieve));
        } 

        #endregion

    }
}