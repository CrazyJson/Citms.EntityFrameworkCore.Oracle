// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EFCore.Oracle.Scaffolding.Internal;
using EFCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Oracle.Design.Internal
{
    public class OracleDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IScaffoldingProviderCodeGenerator, OracleScaffoldingCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, OracleDatabaseModelFactory>()
                .AddSingleton<IAnnotationCodeGenerator, OracleAnnotationCodeGenerator>()
                .AddSingleton<IRelationalTypeMapper, OracleTypeMapper>();
        }
    }
}
