// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace EFCore.Oracle.Storage.Internal
{
    public class OracleVersion
    {
        public OracleVersion(string serverVersion)
        {
            if (serverVersion.StartsWith("9.2"))
                Version = EFOracleVersion.Oracle9iR2;
            else if (serverVersion.StartsWith("10.1"))
                Version = EFOracleVersion.Oracle10gR1;
            else if (serverVersion.StartsWith("10.2"))
                Version = EFOracleVersion.Oracle10gR2;
            else if (serverVersion.StartsWith("11.1"))
                Version = EFOracleVersion.Oracle11gR1;
            else if (serverVersion.StartsWith("11.2"))
                Version = EFOracleVersion.Oracle11gR2;
            else if (serverVersion.StartsWith("12.1"))
                Version = EFOracleVersion.Oracle12cR1;
            else if (serverVersion.StartsWith("12.2"))
                Version = EFOracleVersion.Oracle12cR2;
            else
                throw new InvalidOperationException($"Oracle Data Provider for .NET Not Supported The Oracle Version '${serverVersion}'");

        }

        public readonly EFOracleVersion Version;

        public string VersionHint
        {
            get
            {
                switch (Version)
                {
                    case EFOracleVersion.Oracle11gR1:
                        return "11.1";
                    case EFOracleVersion.Oracle11gR2:
                        return "11.2";
                    case EFOracleVersion.Oracle12cR1:
                        return "12.1";
                    case EFOracleVersion.Oracle12cR2:
                        return "12.2";
                    case EFOracleVersion.Oracle9iR2:
                        return "9.2";
                    case EFOracleVersion.Oracle10gR1:
                        return "10.1";
                    case EFOracleVersion.Oracle10gR2:
                        return "10.2";
                    default:
                        throw new ArgumentException("ODP invalid value ProviderManifestToken");
                }
            }
        }


        public bool IsVersionX
        {
            get
            {
                if (Version != EFOracleVersion.Oracle10gR1 && Version != EFOracleVersion.Oracle10gR2 && (Version != EFOracleVersion.Oracle11gR1 && Version != EFOracleVersion.Oracle11gR2) && (Version != EFOracleVersion.Oracle12cR1 && Version != EFOracleVersion.Oracle12cR2))
                    return Version == EFOracleVersion.Oracle9iR2;
                return true;
            }
        }
    }
    public enum EFOracleVersion
    {
        Oracle9iR2 = 92,
        Oracle10gR1 = 101,
        Oracle10gR2 = 102,
        Oracle11gR1 = 111,
        Oracle11gR2 = 112,
        Oracle12cR1 = 121,
        Oracle12cR2 = 122,
    }
}
