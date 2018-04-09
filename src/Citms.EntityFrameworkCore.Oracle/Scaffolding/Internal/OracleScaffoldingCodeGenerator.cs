// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding;

namespace EFCore.Oracle.Scaffolding.Internal
{
    public class OracleScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
    {
        public virtual string GenerateUseProvider(string connectionString, string language)
        {
            return $".UseOracle(\"{connectionString}\")";
        }
    }
}
