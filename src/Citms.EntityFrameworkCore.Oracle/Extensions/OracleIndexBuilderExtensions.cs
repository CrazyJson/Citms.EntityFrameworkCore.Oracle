// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{

    public static class OracleIndexBuilderExtensions
    {
        public static IndexBuilder ForOracleIsFullText([NotNull] this IndexBuilder indexBuilder, bool fullText = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.Oracle().IsFullText = fullText;

            return indexBuilder;
        }

        public static IndexBuilder ForOracleIsSpatial([NotNull] this IndexBuilder indexBuilder, bool Spatial = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.Oracle().IsSpatial = Spatial;

            return indexBuilder;
        }
    }
}
