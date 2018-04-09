// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Oracle.Storage.Internal
{
    public class OracleRelationalTransaction : RelationalTransaction
    {
        private readonly IRelationalConnection _relationalConnection;
        private readonly DbTransaction _dbTransaction;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> _logger;
        private readonly bool _transactionOwned;

        private bool _connectionClosed;

        public OracleRelationalTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            : base(connection, transaction, logger, transactionOwned)
        {
            if (connection.DbConnection != transaction.Connection)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAssociatedWithDifferentConnection);
            }

            _relationalConnection = connection;
            _dbTransaction = transaction;
            _logger = logger;
            _transactionOwned = transactionOwned;
        }

        public override void Commit()
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _dbTransaction.Commit();
                _logger.TransactionCommitted(
                    _relationalConnection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                _logger.TransactionError(
                    _relationalConnection,
                    _dbTransaction,
                    TransactionId,
                    "CommitAsync",
                    e,
                    startTime,
                    stopwatch.Elapsed);
                throw;
            }

            ClearTransaction();
        }

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        public override void Rollback()
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _dbTransaction.Rollback();

                _logger.TransactionRolledBack(
                    _relationalConnection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                _logger.TransactionError(
                    _relationalConnection,
                    _dbTransaction,
                    TransactionId,
                    "RollbackAsync",
                    e,
                    startTime,
                    stopwatch.Elapsed);
                throw;
            }

            ClearTransaction();
        }

        private void ClearTransaction()
        {
            Debug.Assert(_relationalConnection.CurrentTransaction == null || _relationalConnection.CurrentTransaction == this);

            _relationalConnection.UseTransaction(null);

            if (!_connectionClosed)
            {
                _connectionClosed = true;

                _relationalConnection.Close();
            }
        }
    }
}
