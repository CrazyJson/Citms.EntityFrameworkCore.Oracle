// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Citms.EntityFrameworkCore.Oracle.Properties;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Oracle.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleDatabaseModelFactory : IDatabaseModelFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

        private const string NamePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000.0));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleDatabaseModelFactory(IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DatabaseModel Create(string connectionString, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));

            using (var connection = new OracleConnection(connectionString))
            {
                return Create(connection, tables, schemas);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DatabaseModel Create(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            // for bebugging -- while (!System.Diagnostics.Debugger.IsAttached) { }
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));

            var databaseModel = new DatabaseModel();

            var connectionStartedOpen = connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connection.Open();
            }
            try
            {
                databaseModel.DefaultSchema = GetDefaultSchema(connection);

                var schemaList = schemas.ToList();
                var schemaFilter = GenerateSchemaFilter(schemaList);
                var tableList = tables.ToList();
                var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

                GetTables(connection, tableFilter, databaseModel.DefaultSchema, schemaFilter, databaseModel, schemaList);

                foreach (var schema in schemaList
                    .Except(
                        databaseModel.Sequences.Select(s => s.Schema)
                            .Concat(databaseModel.Tables.Select(t => t.Schema))))
                {
                    _logger.MissingSchemaWarning(schema);
                }

                foreach (var table in tableList)
                {
                    var (Schema, Table) = Parse(table);
                    if (!databaseModel.Tables.Any(
                        t => !string.IsNullOrEmpty(Schema)
                             && t.Schema == Schema
                             || t.Name == Table))
                    {
                        _logger.MissingTableWarning(table);
                    }
                }

                return databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    connection.Close();
                }
            }
        }

        private void GetTables(
            DbConnection connection,
            Func<string, string, string> tableFilter,
            string defaultSchema,
            Func<string, string> schemaFilter,
            DatabaseModel databaseModel,
            List<string> schemaList)
        {
            using (var command = connection.CreateCommand())
            {
                var filter =
                  $"WHERE (t.table_name <> '{HistoryRepository.DefaultTableName}' AND t.owner = '{defaultSchema}' {(tableFilter != null ? $@" AND {tableFilter("t.owner", "t.table_name")})" : ")")}";

                var filterSchema =
                    $"WHERE t.table_name <> '{HistoryRepository.DefaultTableName}' AND t.owner <> '{defaultSchema}' {(schemaFilter != null ? $@"AND {schemaFilter("t.owner")}" : "AND 1=2")} AND rpc.table_name = t.table_name";
                var commandText = $@"
SELECT 
    t.owner AS schema,
    t.table_name AS name
FROM all_tables t
  {filter} 
UNION ALL
SELECT 
    t.owner AS schema,
    t.table_name AS name
FROM all_tables t
  WHERE EXISTS(
     SELECT 1 
       FROM all_constraints rc 
         INNER JOIN all_cons_columns rpc
            ON rpc.constraint_name = rc.r_constraint_name AND rpc.owner = rc.r_owner
        {filterSchema}    
    )";

                command.CommandText = commandText;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schema = reader.GetValueOrDefault<string>("schema");
                        var name = reader.GetValueOrDefault<string>("name");

                        _logger.TableFound(DisplayName(schema, name));

                        var table = new DatabaseTable
                        {
                            Schema = schema,
                            Name = name
                        };

                        databaseModel.Tables.Add(table);
                    }
                }

                var schemaFilter2 = GenerateSchemaFilter2(schemaList, databaseModel.DefaultSchema);
                var tableFilter2 = GenerateTableFilter2(databaseModel.Tables.ToList<DatabaseTable>());

                filter =
                    $"WHERE (t.table_name <> '{HistoryRepository.DefaultTableName}' {(schemaFilter2 != null ? $@"AND {schemaFilter2("t.owner")}" : "")} {(tableFilter2 != null ? $@" AND {tableFilter2("t.owner", "t.table_name")}" : "")})";

                GetColumns(connection, filter, databaseModel);
                GetKeys(connection, filter, databaseModel);
                GetIndexes(connection, filter, databaseModel);
                GetForeignKeys(connection, filter, databaseModel);
            }
        }

        private void GetColumns(
            DbConnection connection,
            string tableFilter,
            DatabaseModel databaseModel)
        {
            using (var command = (OracleCommand)connection.CreateCommand())
            {
                command.InitialLONGFetchSize = -1;

                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT")
                    .AppendLine("   t.owner,")
                    .AppendLine("   c.table_name,")
                    .AppendLine("   c.column_name,")
                    .AppendLine("   c.column_id,")
                    .AppendLine("   c.data_type,")
                    .AppendLine("   c.data_length,")
                    .AppendLine("   c.data_precision,")
                    .AppendLine("   c.data_scale,")
                    .AppendLine("   c.nullable,")
                    .AppendLine("   'NO' identity_column,")
                    .AppendLine("   c.data_default,")
                    .AppendLine("   c.virtual_column")
                    .AppendLine("FROM all_tab_cols c")
                    .AppendLine("  INNER JOIN all_tables t ")
                    .AppendLine("    ON UPPER(t.table_name)=UPPER(c.table_name) AND t.owner = c.owner")
                    .AppendLine(tableFilter)
                    .AppendLine("ORDER BY t.owner")
                    .ToString();

                    using (var reader = command.ExecuteReader())
                {
                    var tableColumnGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("owner"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableColumnGroup in tableColumnGroups)
                    {
                        var tableSchema = tableColumnGroup.Key.tableSchema;
                        var tableName = tableColumnGroup.Key.tableName;
                        var table = (DatabaseTable) null;
                        try
                        {
                             table = databaseModel.Tables.Single(t => t.Schema == tableSchema && t.Name == tableName) ;
                        } catch 
                        {
                             table = null;
                        }

                        if (table != null) { 
                        foreach (var dataRecord in tableColumnGroup)
                        {
                                if (table != null)
                                {
                                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                    var ordinal = dataRecord.GetValueOrDefault<int>("column_id");
                                    var dataTypeName = dataRecord.GetValueOrDefault<string>("data_type");
                                    var maxLength = dataRecord.GetValueOrDefault<int>("data_length");
                                    var precision = dataRecord.GetValueOrDefault<int>("data_precision");
                                    var scale = dataRecord.GetValueOrDefault<int>("data_scale");
                                    var isNullable = dataRecord.GetValueOrDefault<string>("nullable").Equals("Y");
                                    var isIdentity = dataRecord.GetValueOrDefault<string>("identity_column").Equals("YES");
                                    var defaultValue = !isIdentity ? dataRecord.GetValueOrDefault<string>("data_default") : null;
                                    var computedValue = dataRecord.GetValueOrDefault<string>("virtual_column").Equals("YES") ? defaultValue : null;

                                    var storeType = GetOracleClrType(dataTypeName, maxLength, precision, scale);
                                    if (string.IsNullOrWhiteSpace(defaultValue)
                                        || !string.IsNullOrWhiteSpace(computedValue))
                                    {
                                        defaultValue = null;
                                    }

                                    _logger.ColumnFound(
                                        DisplayName(tableSchema, tableName),
                                        columnName,
                                        ordinal,
                                        dataTypeName,
                                        maxLength,
                                        precision,
                                        scale,
                                        isNullable,
                                        isIdentity,
                                        defaultValue,
                                        computedValue);

                                    var column = new DatabaseColumn
                                    {
                                        Table = table,
                                        Name = columnName,
                                        StoreType = storeType,
                                        IsNullable = isNullable,
                                        DefaultValueSql = defaultValue,
                                        ComputedColumnSql = computedValue,
                                        ValueGenerated = isIdentity
                                            ? ValueGenerated.OnAdd
                                            : default(ValueGenerated?)
                                    };

                                    table.Columns.Add(column);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetKeys(
            DbConnection connection,
            string tableFilter,
            DatabaseModel databaseModel)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT")
                    .AppendLine("   t.owner,")
                    .AppendLine("   c.table_name,")
                    .AppendLine("   cc.column_name,")
                    .AppendLine("   c.delete_rule,")
                    .AppendLine("   c.constraint_name,")
                    .AppendLine("   c.constraint_type")
                    .AppendLine("FROM all_constraints c")
                    .AppendLine("  INNER JOIN all_tables t")
                    .AppendLine("    ON UPPER(t.table_name) = UPPER(c.table_name) and t.owner = c.owner")
                    .AppendLine("  INNER JOIN all_cons_columns cc")
                    .AppendLine("    ON cc.constraint_name = c.constraint_name and cc.owner = t.owner")
                    .AppendLine(tableFilter)
                    .AppendLine(" AND c.constraint_type IN ('P','U') ")
                    .AppendLine("ORDER BY t.owner")
                    .ToString();

                using (var reader = command.ExecuteReader())
                {
                    var tableIndexGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            //ddr => (tableSchema: ddr.GetValueOrDefault<string>("tablespace_name"),
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("owner"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableIndexGroup in tableIndexGroups)
                    {
                        var tableSchema = tableIndexGroup.Key.tableSchema;
                        var tableName = tableIndexGroup.Key.tableName;

                        var table = (DatabaseTable)null;
                        try
                        {
                            table = databaseModel.Tables.Single(t => t.Schema == tableSchema && t.Name == tableName);
                        }
                        catch
                        {
                            table = null;
                        }

                        if (table != null)
                        {
                            var primaryKeyGroups = tableIndexGroup
                            .Where(ddr => ddr.GetValueOrDefault<string>("constraint_type").Equals("P"))
                            .GroupBy(ddr => ddr.GetValueOrDefault<string>("constraint_name"))
                            .ToArray();

                            if (primaryKeyGroups.Length == 1)
                            {
                                var primaryKeyGroup = primaryKeyGroups[0];

                                _logger.PrimaryKeyFound(primaryKeyGroup.Key, DisplayName(tableSchema, tableName));

                                var primaryKey = new DatabasePrimaryKey
                                {
                                    Table = table,
                                    Name = primaryKeyGroup.Key
                                };

                                foreach (var dataRecord in primaryKeyGroup)
                                {
                                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                                 ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                                    primaryKey.Columns.Add(column);
                                }

                                table.PrimaryKey = primaryKey;
                            }

                            var uniqueConstraintGroups = tableIndexGroup
                                .Where(ddr => ddr.GetValueOrDefault<string>("constraint_type").Equals("U"))
                                .GroupBy(ddr => ddr.GetValueOrDefault<string>("constraint_name"))
                                .ToArray();

                            foreach (var uniqueConstraintGroup in uniqueConstraintGroups)
                            {
                                _logger.UniqueConstraintFound(uniqueConstraintGroup.Key, DisplayName(tableSchema, tableName));

                                var uniqueConstraint = new DatabaseUniqueConstraint
                                {
                                    Table = table,
                                    Name = uniqueConstraintGroup.Key
                                };

                                foreach (var dataRecord in uniqueConstraintGroup)
                                {
                                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                                 ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                                    uniqueConstraint.Columns.Add(column);
                                }

                                table.UniqueConstraints.Add(uniqueConstraint);
                            }
                        }
                    }
                }
            }
        }

        private void GetForeignKeys(
            DbConnection connection,
            string tableFilter,
            DatabaseModel databaseModel)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = new StringBuilder()
                     .AppendLine("SELECT")
                     .AppendLine("  t.owner,")
                     .AppendLine("  ric.constraint_name,")
                     .AppendLine("  ric.table_name,")
                     .AppendLine("  rc.column_name,")
                     .AppendLine("  rpc.column_name principal_column_name,")
                     .AppendLine("  rpc.table_name principal_table_name,")
                     .AppendLine("  ric.r_owner principal_schema,")
                     .AppendLine("  ric.delete_rule")
                     .AppendLine("FROM all_constraints ric")
                     .AppendLine("  INNER JOIN all_tables t")
                     .AppendLine("    ON t.table_name = ric.table_name AND t.owner = ric.owner")
                     .AppendLine("  INNER JOIN all_cons_columns rc")
                     .AppendLine("    ON rc.constraint_name = ric.constraint_name AND rc.table_name = ric.table_name AND rc.owner = ric.owner")
                     .AppendLine("  INNER JOIN all_cons_columns rpc")
                     .AppendLine("    ON rpc.constraint_name = ric.r_constraint_name AND rpc.owner = ric.r_owner")
                     .AppendLine(tableFilter)
                     .AppendLine("ORDER BY ric.owner, ric.constraint_name, ric.table_name")
                     .ToString();

                using (var reader = command.ExecuteReader())
                {
                    var tableForeignKeyGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            //ddr => (tableSchema: ddr.GetValueOrDefault<string>("tablespace_name"),
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("owner"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableForeignKeyGroup in tableForeignKeyGroups)
                    {
                        var tableSchema = tableForeignKeyGroup.Key.tableSchema;
                        var tableName = tableForeignKeyGroup.Key.tableName;

                        var table = (DatabaseTable)null;
                        try
                        {
                            table = databaseModel.Tables.Single(t => t.Schema == tableSchema && t.Name == tableName);
                        }
                        catch
                        {
                            table = null;
                        }

                        if (table != null)
                        {
                            var foreignKeyGroups = tableForeignKeyGroup
                                .GroupBy(
                                    c => (Name: c.GetValueOrDefault<string>("constraint_name"),
                                        //Amer PrincipalTableSchema: c.GetValueOrDefault<string>("tablespace_name"),
                                        PrincipalTableSchema: c.GetValueOrDefault<string>("principal_schema"),
                                        PrincipalTableName: c.GetValueOrDefault<string>("principal_table_name"),
                                        OnDeleteAction: c.GetValueOrDefault<string>("delete_rule")));

                            foreach (var foreignKeyGroup in foreignKeyGroups)
                            {
                                var fkName = foreignKeyGroup.Key.Name;
                                var principalTableSchema = foreignKeyGroup.Key.PrincipalTableSchema;
                                var principalTableName = foreignKeyGroup.Key.PrincipalTableName;
                                var onDeleteAction = foreignKeyGroup.Key.OnDeleteAction;

                                _logger.ForeignKeyFound(
                                    fkName,
                                    DisplayName(table.Schema, table.Name),
                                    DisplayName(principalTableSchema, principalTableName),
                                    onDeleteAction);

                                var principalTable = databaseModel.Tables.FirstOrDefault(
                                                         t => t.Schema == principalTableSchema
                                                              && t.Name == principalTableName)
                                                     ?? databaseModel.Tables.FirstOrDefault(
                                                         t => t.Schema.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase)
                                                              && t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                                if (principalTable == null)
                                {
                                    _logger.ForeignKeyReferencesMissingPrincipalTableWarning(
                                        fkName,
                                        DisplayName(table.Schema, table.Name),
                                        DisplayName(principalTableSchema, principalTableName));

                                    continue;
                                }

                                var foreignKey = new DatabaseForeignKey
                                {
                                    Name = fkName,
                                    Table = table,
                                    PrincipalTable = principalTable,
                                    OnDelete = ConvertToReferentialAction(onDeleteAction)
                                };

                                var invalid = false;

                                foreach (var dataRecord in foreignKeyGroup)
                                {
                                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                                 ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                                    var principalColumnName = dataRecord.GetValueOrDefault<string>("principal_column_name");
                                    var principalColumn = foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                                                          ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
                                    if (principalColumn == null)
                                    {
                                        invalid = true;
                                        _logger.ForeignKeyPrincipalColumnMissingWarning(
                                            fkName,
                                            DisplayName(table.Schema, table.Name),
                                            principalColumnName,
                                            DisplayName(principalTableSchema, principalTableName));
                                        break;
                                    }

                                    foreignKey.Columns.Add(column);
                                    foreignKey.PrincipalColumns.Add(principalColumn);
                                }

                                if (!invalid)
                                {
                                    table.ForeignKeys.Add(foreignKey);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetIndexes(
            DbConnection connection,
            string tableFilter,
            DatabaseModel databaseModel)
        {
            using (var command = connection.CreateCommand())
            {
                var queryBuilder = new StringBuilder()
                    .AppendLine("SELECT")
                    .AppendLine("   t.owner,")
                    .AppendLine("   i.uniqueness,")
                    .AppendLine("   i.index_name,")
                    .AppendLine("   i.table_name,")
                    .AppendLine("   ic.column_name")
                    .AppendLine("FROM all_indexes i")
                    .AppendLine("INNER JOIN all_tables t")
                    .AppendLine("   ON t.table_name = i.table_name AND t.owner = i.owner")
                    .AppendLine("INNER JOIN all_ind_columns ic")
                    .AppendLine("   ON ic.index_name = i.index_name AND ic.index_owner = t.owner")
                    .AppendLine(tableFilter)
                    .AppendLine("ORDER BY t.owner, i.table_name, i.index_name, ic.column_position");

                command.CommandText = queryBuilder.ToString();

                using (var reader = command.ExecuteReader())
                {
                    var tableIndexGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            //Amerddr => (tableSchema: ddr.GetValueOrDefault<string>("tablespace_name"),
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("owner"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableIndexGroup in tableIndexGroups)
                    {
                        var tableSchema = tableIndexGroup.Key.tableSchema;
                        var tableName = tableIndexGroup.Key.tableName;

                        var table = (DatabaseTable)null;
                        try
                        {
                            table = databaseModel.Tables.Single(t => t.Schema == tableSchema && t.Name == tableName);
                        }
                        catch
                        {
                            table = null;
                        }

                        if (table != null)
                        {
                            var indexGroups = tableIndexGroup
                                .GroupBy(
                                    ddr =>
                                        (Name: ddr.GetValueOrDefault<string>("index_name"),
                                        IsUnique: ddr.GetValueOrDefault<string>("uniqueness").Equals("UNIQUE")))
                                .ToArray();

                            foreach (var indexGroup in indexGroups)
                            {
                                _logger.IndexFound(indexGroup.Key.Name, DisplayName(tableSchema, tableName), indexGroup.Key.IsUnique);

                                var index = new DatabaseIndex
                                {
                                    Table = table,
                                    Name = indexGroup.Key.Name,
                                    IsUnique = indexGroup.Key.IsUnique
                                };

                                foreach (var dataRecord in indexGroup)
                                {
                                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                                 ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                                    index.Columns.Add(column);
                                }

                                table.Indexes.Add(index);
                            }
                        }
                    }
                }
            }
        }

        private static string DisplayName(string schema, string name)
            => (!string.IsNullOrEmpty(schema) ? schema + "." : "") + name;

        private string GetDefaultSchema(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                //Amer command.CommandText = "SELECT default_tablespace FROM user_users";
                command.CommandText = "SELECT username FROM user_users";

                if (command.ExecuteScalar() is string schema)
                {
                    _logger.DefaultSchemaFound(schema);

                    return schema;
                }

                return null;
            }
        }

        private static string GetOracleClrType(string dataTypeName, int maxLength, int precision, int scale)
        {
            switch (dataTypeName.ToUpper())
            {
                case "DECIMAL":
                case "NUMERIC":
                case "NUMBER":
                    {
                        if (precision == 0
                            && scale == 0)
                        {
                            precision = 10;
                        }
                        else if (precision > 10
                                 && scale > 0)
                        {
                            precision = 29;
                            scale = 4;
                        }
                        else if (precision < 6
                                 && scale == 0)
                        {
                            precision = 6;
                        }
                        return scale > 0
                            ? $"{dataTypeName}({precision},{scale})"
                            : $"{dataTypeName}({precision})";
                    }
                case "NVARCHAR2":
                case "NVARCHAR":
                case "VARCHAR":
                case "NCHAR":
                case "CHAR":
                case "NCLOB":
                case "CLOB":
                    {
                        if (maxLength < 0)
                        {
                            return $"{dataTypeName}(4000)";
                        }
                        return $"{dataTypeName}({maxLength})";
                    }
            }

            return dataTypeName;
        }

        private static ReferentialAction? ConvertToReferentialAction(string onDeleteAction)
        {
            switch (onDeleteAction)
            {
                case "NO ACTION":
                    return ReferentialAction.NoAction;
                case "CASCADE":
                    return ReferentialAction.Cascade;
                default:
                    return null;
            }
        }

        private static Func<string, string> GenerateSchemaFilter(IReadOnlyList<string> schemas)
        {
            if (schemas.Count > 0)
            {
                return s =>
                {
                    var schemaFilterBuilder = new StringBuilder();
                    schemaFilterBuilder.Append(s);
                    schemaFilterBuilder.Append(" IN (");
                    schemaFilterBuilder.Append(string.Join(", ", schemas.Select(EscapeLiteral)).ToUpper());
                    schemaFilterBuilder.Append(")");
                    return schemaFilterBuilder.ToString();
                };
            }
            return null;
        }

        private static Func<string, string> GenerateSchemaFilter2(IReadOnlyList<string> schemas, string defaultSchema)
        {
            if (schemas.Count > 0)
            {
                return s =>
                {
                    var schemaFilterBuilder = new StringBuilder();
                    schemaFilterBuilder.Append(s);
                    schemaFilterBuilder.Append(" IN (");
                    schemaFilterBuilder.Append(EscapeLiteral(defaultSchema.ToUpper()) + (schemas.Count > 0? ", " : ""));
                    schemaFilterBuilder.Append(string.Join(", ", schemas.Select(EscapeLiteral)).ToUpper());
                    schemaFilterBuilder.Append(")");
                    return schemaFilterBuilder.ToString();
                };
            }
            return null;
        }

        private static (string Schema, string Table) Parse(string table)
        {
            var match = _partExtractor.Match(table.Trim());

            if (!match.Success)
            {
                throw new InvalidOperationException(OracleStrings.InvalidTableToIncludeInScaffolding(table));
            }

            var part1 = match.Groups["part1"].Value.Replace("]]", "]");
            var part2 = match.Groups["part2"].Value.Replace("]]", "]");

            return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
        }

        private static Func<string, string, string> GenerateTableFilter2(
            IList<DatabaseTable> tables)
        {
            if (tables.Count > 0)
            {
                return (s, t) =>
                {
                    var tableFilterBuilder = new StringBuilder();

                    var openBracket = false;
                    if (tables.Count > 0)
                    {
                        if (openBracket)
                        {
                            tableFilterBuilder
                                .AppendLine()
                                .Append("OR ");
                        }
                        else
                        {
                            tableFilterBuilder.Append("(");
                            openBracket = true;
                        }

                        var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithoutSchema.Count > 0)
                        {
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Name))).ToUpper());
                            tableFilterBuilder.Append(")");
                        }

                        var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithSchema.Count > 0)
                        {
                            if (tablesWithoutSchema.Count > 0)
                            {
                                tableFilterBuilder.Append(" OR ");
                            }
                            tableFilterBuilder.Append(s);
                            tableFilterBuilder.Append("||N'.'||");
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Name}"))).ToUpper());
                            tableFilterBuilder.Append(")");
                        }
                    }

                    if (openBracket)
                    {
                        tableFilterBuilder.Append(")");
                    }

                    return tableFilterBuilder.ToString();
                };
            }

            return null;
        }

        private static Func<string, string, string> GenerateTableFilter(
               IReadOnlyList<(string Schema, string Table)> tables,
              Func<string, string> schemaFilter)
        { 
        
            if (tables.Count > 0)
            {
                return (s, t) =>
                {
                    var tableFilterBuilder = new StringBuilder();

                    var openBracket = false;
                    /*
                    if (schemaFilter != null)
                    {
                        tableFilterBuilder
                            .Append("(")
                            .Append(schemaFilter(s));
                        openBracket = true;
                    }
                    */

                    if (tables.Count > 0)
                    {
                        if (openBracket)
                        {
                            tableFilterBuilder
                                .AppendLine()
                                .Append("OR ");
                        }
                        else
                        {
                            tableFilterBuilder.Append("(");
                            openBracket = true;
                        }

                        var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithoutSchema.Count > 0)
                        {
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table))).ToUpper());
                            tableFilterBuilder.Append(")");
                        }

                        var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithSchema.Count > 0)
                        {
                            if (tablesWithoutSchema.Count > 0)
                            {
                                tableFilterBuilder.Append(" OR ");
                            }
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table))).ToUpper());
                            tableFilterBuilder.Append(") AND CONCAT(");
                            tableFilterBuilder.Append(s);
                            tableFilterBuilder.Append(", N'.', ");
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(") IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Table}"))).ToUpper());
                            tableFilterBuilder.Append(")");
                        }
                    }

                    if (openBracket)
                    {
                        tableFilterBuilder.Append(")");
                    }

                    return tableFilterBuilder.ToString();
                };
            }

            return null;
        }

        private static string EscapeLiteral(string s)
        {
            return $"N'{s}'";
        }
    }
}
