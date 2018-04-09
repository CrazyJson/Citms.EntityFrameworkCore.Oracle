// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;

namespace EFCore.Oracle.Storage.Internal
{
    public class OracleRelationalConnection : RelationalConnection, IOracleRelationalConnection
    {
        public OracleRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override DbConnection CreateDbConnection() => new OracleConnection(ConnectionString);

        public virtual IOracleRelationalConnection CreateMasterConnection()
        {
            var csb = new OracleConnectionStringBuilder(ConnectionString)
            {
                MinPoolSize = 10,
                MaxPoolSize = 100,
                Pooling = true
            };

            var contextOptions = new DbContextOptionsBuilder()
                .UseOracle(csb.ConnectionString)
                .Options;

            return new OracleRelationalConnection(Dependencies.With(contextOptions));
        }

        public override bool IsMultipleActiveResultSetsEnabled => false;

        [NotNull]
        public override IDbContextTransaction BeginTransaction(
            IsolationLevel isolationLevel)
        {
            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
            }
            Open();
            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        private IDbContextTransaction BeginTransactionWithNoPreconditions(IsolationLevel isolationLevel)
        {
            DbTransaction dbTransaction = null;

            dbTransaction = DbConnection.BeginTransaction(isolationLevel);


            CurrentTransaction
                = new OracleRelationalTransaction(
                    this,
                    dbTransaction,
                    Dependencies.TransactionLogger,
                    transactionOwned: true);

            Dependencies.TransactionLogger.TransactionStarted(
                this,
                dbTransaction,
                CurrentTransaction.TransactionId,
                DateTimeOffset.UtcNow);

            return CurrentTransaction;
        }

        /// <summary>
        ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        public override IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            if (transaction == null)
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction = null;
                }
            }
            else
            {
                if (CurrentTransaction != null)
                {
                    throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
                }

                Open();

                CurrentTransaction = new OracleRelationalTransaction(
                    this,
                    transaction,
                    Dependencies.TransactionLogger,
                    transactionOwned: false);

                Dependencies.TransactionLogger.TransactionUsed(
                    this,
                    transaction,
                    CurrentTransaction.TransactionId,
                    DateTimeOffset.UtcNow);
            }

            return CurrentTransaction;
        }

        public override void CommitTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            (CurrentTransaction as OracleRelationalTransaction).Commit();
        }

        public override void RollbackTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }
            (CurrentTransaction as OracleRelationalTransaction).Rollback();
        }
    }
}
