// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Oracle.Query.Sql.Internal
{
    public class OracleQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        public OracleQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression)
            : base(dependencies, selectExpression)
        {
        }

        protected override string TypedTrueLiteral => "1";
        protected override string TypedFalseLiteral => "0";
        protected override string AliasSeparator => " ";

        protected override string GenerateOperator(Expression expression)
            => expression.NodeType == ExpressionType.Add
               && expression.Type == typeof(string)
                ? " || "
                : base.GenerateOperator(expression);

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.And:
                    Sql.Append("BITAND(");

                    Visit(binaryExpression.Left);

                    Sql.Append(", ");

                    Visit(binaryExpression.Right);

                    Sql.Append(")");

                    return binaryExpression;

                case ExpressionType.Or:
                    Visit(binaryExpression.Left);

                    Sql.Append(" - BITAND(");

                    Visit(binaryExpression.Left);

                    Sql.Append(", ");

                    Visit(binaryExpression.Right);

                    Sql.Append(") + ");

                    Visit(binaryExpression.Right);

                    return binaryExpression;

                case ExpressionType.Modulo:
                    Sql.Append("MOD(");

                    Visit(binaryExpression.Left);

                    Sql.Append(", ");

                    Visit(binaryExpression.Right);

                    Sql.Append(")");

                    return binaryExpression;

            }

            return base.VisitBinary(binaryExpression);
        }

        protected override void GenerateOrderBy(IReadOnlyList<Ordering> orderings)
        {
            orderings
                = orderings.Where(
                    o =>
                        o.Expression.NodeType != ExpressionType.Constant
                        && o.Expression.NodeType != ExpressionType.Parameter).ToList();

            if (orderings.Count > 0)
            {
                base.GenerateOrderBy(orderings);
            }
        }

        protected override void GenerateOrdering(Ordering ordering)
        {
            Check.NotNull(ordering, nameof(ordering));

            var orderingExpression = ordering.Expression;

            if (!(orderingExpression.NodeType == ExpressionType.Constant
                  || orderingExpression.NodeType == ExpressionType.Parameter))
            {
                base.GenerateOrdering(ordering);

                if (ordering.OrderingDirection == OrderingDirection.Asc)
                {
                    Sql.Append(" NULLS FIRST");
                }
            }
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // Not supported
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (RequiresRowNumberPaging(selectExpression))
            {
                Sql.AppendLine().Append(") t)").Append(" WHERE ");
                if (selectExpression.Limit != null)
                {
                    Sql.Append("rownum <=");
                    Visit(selectExpression.Limit);
                }
                if (selectExpression.Limit != null)
                {
                    Sql.Append(" and ");
                }
                Sql.Append(" RN >  ");
                if (selectExpression.Offset == null)
                {
                    Sql.Append("0");
                }
                else
                {
                    Visit(selectExpression.Offset);
                }
            }
            else
            {
                base.GenerateLimitOffset(selectExpression);
            }
        }
        private static bool RequiresRowNumberPaging(SelectExpression selectExpression) => selectExpression.Limit != null || selectExpression.Offset != null;
        public override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                Sql.AppendLine("(");

                subQueryIndent = Sql.Indent();
            }

            if (RequiresRowNumberPaging(selectExpression))
            {
                Sql.Append("SELECT * FROM (SELECT t.*,rownum RN  FROM(").AppendLine().Append("    ");
            }

            Sql.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                Sql.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            var projectionAdded = false;

            if (selectExpression.IsProjectStar)
            {
                var tableAlias = selectExpression.ProjectStarTable.Alias;

                Sql
                    .Append(SqlGenerator.DelimitIdentifier(tableAlias))
                    .Append(".*");

                projectionAdded = true;
            }

            if (selectExpression.Projection.Any())
            {
                if (selectExpression.IsProjectStar)
                {
                    Sql.Append(", ");
                }

                ProcessExpressionList(selectExpression.Projection, GenerateProjection);

                projectionAdded = true;
            }

            if (!projectionAdded)
            {
                Sql.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                Sql.AppendLine()
                    .Append("FROM ");

                ProcessExpressionList(selectExpression.Tables, sql => sql.AppendLine());
            }
            else
            {
                Sql.Append(" FROM DUAL");
            }

            if (selectExpression.Predicate != null)
            {
                GeneratePredicate(selectExpression.Predicate);
            }

            if (selectExpression.GroupBy.Any())
            {
                Sql.AppendLine().Append("GROUP BY ");
                ProcessExpressionList(selectExpression.GroupBy);
            }

            if (selectExpression.OrderBy.Any())
            {
                Sql.AppendLine();

                GenerateOrderBy(selectExpression.OrderBy);
            }

            GenerateLimitOffset(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                Sql.AppendLine()
                    .Append(")");

                if (selectExpression.Alias.Length > 0)
                {
                    Sql.Append(" ")
                        .Append(SqlGenerator.DelimitIdentifier(selectExpression.Alias));
                }
            }

            return selectExpression;
        }

        private void ProcessExpressionList(
         IReadOnlyList<Expression> expressions, Action<IRelationalCommandBuilder> joinAction = null)
         => ProcessExpressionList(expressions, e => Visit(e), joinAction);

        private void ProcessExpressionList<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(Sql);
                }

                itemAction(items[i]);
            }
        }

        public override Expression VisitAlias(AliasExpression aliasExpression)
        {
            Check.NotNull(aliasExpression, nameof(aliasExpression));

            Visit(aliasExpression.Expression);

            if (aliasExpression.Alias != null)
            {
                Sql.Append(" ");
            }

            if (aliasExpression.Alias != null)
            {
                Sql.Append(SqlGenerator.DelimitIdentifier(aliasExpression.Alias));
            }

            return aliasExpression;
        }

        public override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            Sql.Append(SqlGenerator.DelimitIdentifier(tableExpression.Table, tableExpression.Schema))
                .Append(" ")
                .Append(SqlGenerator.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        public override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            Sql.AppendLine("(");

            using (Sql.Indent())
            {
                GenerateFromSql(fromSqlExpression.Sql, fromSqlExpression.Arguments, ParameterValues);
            }

            Sql.Append(") ")
                .Append(SqlGenerator.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        protected override void GeneratePseudoFromClause()
        {
            Sql.Append(" FROM DUAL");
        }

        public override Expression VisitCrossJoinLateral(CrossJoinLateralExpression crossJoinLateralExpression)
        {
            Check.NotNull(crossJoinLateralExpression, nameof(crossJoinLateralExpression));

            Sql.Append("CROSS APPLY ");

            Visit(crossJoinLateralExpression.TableExpression);

            return crossJoinLateralExpression;
        }

        private static readonly HashSet<string> _builtInFunctions
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MAX",
                "MIN",
                "SUM",
                "SUBSTR",
                "INSTR",
                "LENGTH",
                "COUNT"
            };

        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            switch (sqlFunctionExpression.FunctionName)
            {
                case "EXTRACT":
                    Sql.Append(sqlFunctionExpression.FunctionName);
                    Sql.Append("(");

                    Visit(sqlFunctionExpression.Arguments[0]);

                    Sql.Append(" FROM ");

                    Visit(sqlFunctionExpression.Arguments[1]);

                    Sql.Append(")");

                    return sqlFunctionExpression;

                case "CAST":
                    Sql.Append(sqlFunctionExpression.FunctionName);
                    Sql.Append("(");

                    Visit(sqlFunctionExpression.Arguments[0]);

                    Sql.Append(" AS ");

                    Visit(sqlFunctionExpression.Arguments[1]);

                    Sql.Append(")");

                    return sqlFunctionExpression;

                case "AVG" when sqlFunctionExpression.Type == typeof(decimal):
                case "SUM" when sqlFunctionExpression.Type == typeof(decimal):
                    Sql.Append("CAST(");

                    base.VisitSqlFunction(sqlFunctionExpression);

                    Sql.Append(" AS NUMBER(29,4))");

                    return sqlFunctionExpression;

                case "INSTR":
                    if (sqlFunctionExpression.Arguments[1] is ParameterExpression parameterExpression
                        && ParameterValues.TryGetValue(parameterExpression.Name, out var value)
                        && ((string)value)?.Length == 0)
                    {
                        return Visit(Expression.Constant(1));
                    }

                    break;

                case "ADD_MONTHS":
                    Sql.Append("CAST(");

                    base.VisitSqlFunction(sqlFunctionExpression);

                    Sql.Append(" AS TIMESTAMP)");

                    return sqlFunctionExpression;

            }

            return base.VisitSqlFunction(
                // non-instance & non-built-in functions without schema needs to be delimited
                (!_builtInFunctions.Contains(sqlFunctionExpression.FunctionName)
                && sqlFunctionExpression.Instance == null)
                    ? sqlFunctionExpression.IsNiladic
                        ? new SqlFunctionExpression(
                            SqlGenerator.DelimitIdentifier(sqlFunctionExpression.FunctionName),
                            sqlFunctionExpression.Type,
                            sqlFunctionExpression.IsNiladic)
                        : new SqlFunctionExpression(
                            SqlGenerator.DelimitIdentifier(sqlFunctionExpression.FunctionName),
                            sqlFunctionExpression.Type,
                            /* schema:*/ null,
                            sqlFunctionExpression.Arguments)
                    : sqlFunctionExpression);
        }

        protected override void GenerateProjection(Expression projection)
        {
            var aliasedProjection = projection as AliasExpression;
            var expressionToProcess = aliasedProjection?.Expression ?? projection;
            var updatedExperssion = ExplicitCastToBool(expressionToProcess);

            expressionToProcess = aliasedProjection != null
                ? new AliasExpression(aliasedProjection.Alias, updatedExperssion)
                : updatedExperssion;

            var colExp = expressionToProcess as ColumnExpression;
            if (colExp == null)
            {
                base.GenerateProjection(expressionToProcess);
                return;
            }
            var tblExp = colExp.Table as TableExpression;

            if (tblExp.Alias.Contains("."))
            {
                var fieldStr = $"{SqlGenerator.DelimitIdentifier(tblExp.Alias ?? tblExp.Table)}.\"{colExp.Name}\" as {SqlGenerator.DelimitIdentifier($"{tblExp.Alias ?? tblExp.Table}.{colExp.Name}")}  ";

                Sql.Append(fieldStr);
            }
            else
            {
                base.GenerateProjection(expressionToProcess);
            }
        }

        private static Expression ExplicitCastToBool(Expression expression)
            => (expression as BinaryExpression)?.NodeType == ExpressionType.Coalesce
               && expression.Type.UnwrapNullableType() == typeof(bool)
                ? new ExplicitCastExpression(expression, expression.Type)
                : expression;
    }
}
