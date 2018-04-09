// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using EFCore.Oracle.Infrastructure.Internal;
using EFCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;

namespace EFCore.Oracle.Internal
{
    public class OracleOptions : IOracleOptions
    {
        private OracleOptionsExtension _relationalOptions;

        private readonly Lazy<OracleConnectionSettings> _lazyConnectionSettings;

        public OracleOptions()
        {
            _lazyConnectionSettings = new Lazy<OracleConnectionSettings>(() =>
            {
                if (_relationalOptions.Connection != null)
                {
                    return OracleConnectionSettings.GetSettings(_relationalOptions.Connection);
                }

                return OracleConnectionSettings.GetSettings(_relationalOptions.ConnectionString);
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        public virtual void Initialize(IDbContextOptions options)
        {
            _relationalOptions = options.FindExtension<OracleOptionsExtension>() ?? new OracleOptionsExtension();

        }

        public virtual void Validate(IDbContextOptions options)
        {
            if (_relationalOptions.ConnectionString == null && _relationalOptions.Connection == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoConnectionOrConnectionString);
            }
        }

        public virtual OracleConnectionSettings ConnectionSettings => _lazyConnectionSettings.Value;

        public virtual string GetCreateTable(ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            if (_relationalOptions.Connection != null)
            {
                return GetCreateTable(_relationalOptions.Connection, sqlGenerationHelper, table, schema);
            }

            return GetCreateTable(_relationalOptions.ConnectionString, sqlGenerationHelper, table, schema);
        }

        private static string GetCreateTable(string connectionString, ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                return ExecuteCreateTable(connection, sqlGenerationHelper, table, schema);
            }
        }

        private static string GetCreateTable(DbConnection connection, ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            var opened = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                opened = true;
            }
            try
            {
                return ExecuteCreateTable(connection, sqlGenerationHelper, table, schema);
            }
            finally
            {
                if (opened)
                {
                    connection.Close();
                }
            }
        }

        private static string ExecuteCreateTable(DbConnection connection, ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"SHOW CREATE TABLE {sqlGenerationHelper.DelimitIdentifier(table, schema)}";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetFieldValue<string>(1);
                    }
                }
            }
            return null;
        }
    }
}
