// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Oracle specific extension methods for metadata.
    /// </summary>
    public static class OracleMetadataExtensions
    {
        /// <summary>
        ///     Gets the Oracle specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the property. </returns>
        public static OraclePropertyAnnotations Oracle([NotNull] this IMutableProperty property)
            => (OraclePropertyAnnotations)Oracle((IProperty)property);

        /// <summary>
        ///     Gets the Oracle specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the property. </returns>
        public static IOraclePropertyAnnotations Oracle([NotNull] this IProperty property)
            => new OraclePropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     Gets the Oracle specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the entity. </returns>
        public static OracleEntityTypeAnnotations Oracle([NotNull] this IMutableEntityType entityType)
            => (OracleEntityTypeAnnotations)Oracle((IEntityType)entityType);

        /// <summary>
        ///     Gets the Oracle specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the entity. </returns>
        public static IOracleEntityTypeAnnotations Oracle([NotNull] this IEntityType entityType)
            => new OracleEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        /// <summary>
        ///     Gets the Oracle specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the key. </returns>
        public static OracleKeyAnnotations Oracle([NotNull] this IMutableKey key)
            => (OracleKeyAnnotations)Oracle((IKey)key);

        /// <summary>
        ///     Gets the Oracle specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the key. </returns>
        public static IOracleKeyAnnotations Oracle([NotNull] this IKey key)
            => new OracleKeyAnnotations(Check.NotNull(key, nameof(key)));

        /// <summary>
        ///     Gets the Oracle specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the index. </returns>
        public static OracleIndexAnnotations Oracle([NotNull] this IMutableIndex index)
            => (OracleIndexAnnotations)Oracle((IIndex)index);

        /// <summary>
        ///     Gets the Oracle specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the index. </returns>
        public static IOracleIndexAnnotations Oracle([NotNull] this IIndex index)
            => new OracleIndexAnnotations(Check.NotNull(index, nameof(index)));

        /// <summary>
        ///     Gets the Oracle specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the model. </returns>
        public static OracleModelAnnotations Oracle([NotNull] this IMutableModel model)
            => (OracleModelAnnotations)Oracle((IModel)model);

        /// <summary>
        ///     Gets the Oracle specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The Oracle specific metadata for the model. </returns>
        public static IOracleModelAnnotations Oracle([NotNull] this IModel model)
            => new OracleModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
