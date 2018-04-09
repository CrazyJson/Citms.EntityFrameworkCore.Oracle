// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EFCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Oracle.Infrastructure.Internal
{
    public interface IOracleOptions : ISingletonOptions
    {
        OracleConnectionSettings ConnectionSettings { get; }

        string GetCreateTable(ISqlGenerationHelper sqlGenerationHelper, string schema, string table);
    }
}
