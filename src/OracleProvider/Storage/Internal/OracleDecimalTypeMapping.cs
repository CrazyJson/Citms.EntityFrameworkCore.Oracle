// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDecimalTypeMapping : DecimalTypeMapping
    {
        public OracleDecimalTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null) : base(storeType, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size) => new OracleDecimalTypeMapping(storeType);
    }
}
