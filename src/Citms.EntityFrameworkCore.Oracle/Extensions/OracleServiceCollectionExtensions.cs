// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EFCore.Oracle.Infrastructure.Internal;
using EFCore.Oracle.Internal;
using EFCore.Oracle.Migrations.Internal;
using EFCore.Oracle.Query.ExpressionTranslators.Internal;
using EFCore.Oracle.Query.Sql.Internal;
using EFCore.Oracle.Storage.Internal;
using EFCore.Oracle.Update.Internal;
using EFCore.Oracle.ValueGeneration.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OracleServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkOracle([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<OracleOptionsExtension>>()
                .TryAdd<IRelationalTypeMapper, OracleSmartTypeMapper>()
                .TryAdd<ISqlGenerationHelper, OracleSqlGenerationHelper>()
                .TryAdd<IMigrationsAnnotationProvider, OracleMigrationsAnnotationProvider>()
                .TryAdd<IConventionSetBuilder, OracleConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator>(p => p.GetService<IOracleUpdateSqlGenerator>())
                .TryAdd<IModificationCommandBatchFactory, OracleModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, OracleValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetService<IOracleRelationalConnection>())
                .TryAdd<IRelationalCommandBuilderFactory, OracleCommandBuilderFactory>()
                .TryAdd<IMigrationsSqlGenerator, OracleMigrationsSqlGenerator>()
                .TryAdd<IBatchExecutor, OracleBatchExecutor>()
                .TryAdd<IRelationalDatabaseCreator, OracleDatabaseCreator>()
                .TryAdd<IHistoryRepository, OracleHistoryRepository>()
                .TryAdd<IExecutionStrategyFactory, OracleExecutionStrategyFactory>()
                .TryAdd<IMemberTranslator, OracleCompositeMemberTranslator>()
                .TryAdd<ICompositeMethodCallTranslator, OracleCompositeMethodCallTranslator>()
                .TryAdd<IQuerySqlGeneratorFactory, OracleQuerySqlGeneratorFactory>()
                .TryAdd<ISingletonOptions, IOracleOptions>(p => p.GetService<IOracleOptions>())
                .TryAddProviderSpecificServices(b => b
                    .TryAddSingleton<IOracleOptions, OracleOptions>()
                    .TryAddScoped<IOracleUpdateSqlGenerator, OracleUpdateSqlGenerator>()
                    .TryAddScoped<IOracleRelationalConnection, OracleRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
