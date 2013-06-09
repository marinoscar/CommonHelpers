using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Common.Helpers
{
    public class LocalContextTransactionResolver : ITransactionResolver
    {

        public LocalContextTransactionResolver(string connectionString) : this(connectionString, Database.DefaultProvider)
        {
        }

        public LocalContextTransactionResolver(string connectionString, DatabaseProviderType providerType)
        {
            ProviderType = providerType;
            DbProviderFactory factory;
            factory = providerType == DatabaseProviderType.None ? Database.GetDefaultFactory() : Database.GetFactoryFromProvider(providerType);
            Connection = factory.CreateConnection();
            Connection.ConnectionString = connectionString;
        }

        public LocalContextTransactionResolver(DbConnection connection)
        {
            Connection = connection;
        }

        public DbTransaction GetTransactionOrNull()
        {
            if (Transaction == null) Transaction = Connection.BeginTransaction();
            return Transaction;
        }

        public DbConnection GetConnectionOrNull()
        {
            return Connection;
        }

        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public DatabaseProviderType ProviderType { get; private set; }


    }
}
