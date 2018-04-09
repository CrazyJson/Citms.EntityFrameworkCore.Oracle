// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EFCore.Oracle.Metadata.Internal;
using JetBrains.Annotations;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleModelAnnotations : RelationalModelAnnotations, IOracleModelAnnotations
    {
        public OracleModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        protected OracleModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual OracleValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (OracleValueGenerationStrategy?)Annotations.Metadata[OracleAnnotationNames.ValueGenerationStrategy];

            set => SetValueGenerationStrategy(value);
        }

        protected virtual bool SetValueGenerationStrategy(OracleValueGenerationStrategy? value)
            => Annotations.SetAnnotation(OracleAnnotationNames.ValueGenerationStrategy, value);
    }
}
