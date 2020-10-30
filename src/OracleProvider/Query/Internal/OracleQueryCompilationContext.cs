// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Oracle.Query.Internal
{
    public class OracleQueryCompilationContext : RelationalQueryCompilationContext
    {
        private readonly ISet<string> _tableAliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public OracleQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            bool trackQueryResults)
            : base(
                dependencies,
                linqOperatorProvider,
                queryMethodProvider,
                trackQueryResults)
        {
        }

        public override bool IsLateralJoinSupported => true;

        public override int MaxTableAliasLength => 5;

        private string GetRandomLetter(Random rng)
        {
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int index = rng.Next(letters.Length);
            return letters[index].ToString();
        }

        private string GetUniqueTableAlias()
        {
            var rdm = new Random();

            return GetRandomLetter(rdm) + rdm.Next(0, 10);
        }

        public override string CreateUniqueTableAlias([NotNull] string currentAlias)
        {
            Check.NotNull(currentAlias, nameof(currentAlias));

            if (currentAlias.Length == 0)
            {
                return currentAlias;
            }

            var uniqueAlias = GetUniqueTableAlias();

            while (_tableAliasSet.Contains(uniqueAlias))
            {
                uniqueAlias = GetUniqueTableAlias();
            }

            _tableAliasSet.Add(uniqueAlias);

            return uniqueAlias;
        }
    }
}
