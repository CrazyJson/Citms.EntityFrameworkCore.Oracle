// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using EFCore.Oracle.Query.Expressions.Internal;
using JetBrains.Annotations;

namespace EFCore.Oracle.Query.Sql.Internal
{
    public interface IOracleExpressionVisitor
    {
        Expression VisitRegexp([NotNull] RegexpExpression regexpExpression);
    }
}
