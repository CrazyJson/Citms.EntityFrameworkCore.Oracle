// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace EFCore.Oracle.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new OracleContainsOptimizedTranslator(),
            new OracleConvertTranslator(),
            new OracleDateAddTranslator(),
            new OracleEndsWithOptimizedTranslator(),
            new OracleMathTranslator(),
            new OracleNewGuidTranslator(),
            new OracleObjectToStringTranslator(),
            new OracleRegexIsMatchTranslator(),
            new OracleStartsWithOptimizedTranslator(),
            new OracleStringIsNullOrWhiteSpaceTranslator(),
            new OracleStringReplaceTranslator(),
            new OracleStringSubstringTranslator(),
            new OracleStringToLowerTranslator(),
            new OracleStringToUpperTranslator(),
            new OracleStringTrimEndTranslator(),
            new OracleStringTrimStartTranslator(),
            new OracleStringTrimTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
