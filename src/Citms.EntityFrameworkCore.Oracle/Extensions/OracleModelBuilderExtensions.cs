// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Oracle specific extension methods for <see cref="ModelBuilder" />.
    /// </summary>
    public static class OracleModelBuilderExtensions
    {
        /// <summary>
        ///     Configures the model to use the Oracle IDENTITY feature to generate values for key properties
        ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Oracle. This is the default
        ///     behavior when targeting Oracle.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForOracleUseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.IdentityColumn;

            return modelBuilder;
        }

        public static ModelBuilder ForOracleUseComputedColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.ComputedColumn;

            return modelBuilder;
        }
    }
}
