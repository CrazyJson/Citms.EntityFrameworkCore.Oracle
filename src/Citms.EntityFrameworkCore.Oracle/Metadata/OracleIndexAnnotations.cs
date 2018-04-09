// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EFCore.Oracle.Metadata.Internal;
using JetBrains.Annotations;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleIndexAnnotations : RelationalIndexAnnotations, IOracleIndexAnnotations
    {
        public OracleIndexAnnotations([NotNull] IIndex index)
            : base(index)
        {
        }

        protected OracleIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool? IsFullText
        {
            get => (bool?)Annotations.Metadata[OracleAnnotationNames.FullTextIndex];
            set => SetIsFullText(value);
        }

        protected virtual bool SetIsFullText(bool? value) => Annotations.SetAnnotation(
            OracleAnnotationNames.FullTextIndex,
            value);

        public virtual bool? IsSpatial
        {
            get => (bool?)Annotations.Metadata[OracleAnnotationNames.SpatialIndex];
            set => SetIsSpatial(value);
        }

        protected virtual bool SetIsSpatial(bool? value) => Annotations.SetAnnotation(
            OracleAnnotationNames.SpatialIndex,
            value);
    }
}
