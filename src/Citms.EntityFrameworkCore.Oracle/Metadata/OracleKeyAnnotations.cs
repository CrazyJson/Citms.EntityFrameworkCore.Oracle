// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleKeyAnnotations : RelationalKeyAnnotations, IOracleKeyAnnotations
    {
        public OracleKeyAnnotations([NotNull] IKey key)
            : base(key)
        {
        }

        protected OracleKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }
    }
}
