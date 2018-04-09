// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace EFCore.Oracle.Storage.Internal
{
    public class OracleTypeMapper : RelationalTypeMapper
    {
        // boolean
        private readonly OracleBoolTypeMapping _bit          = new OracleBoolTypeMapping("bit", DbType.Boolean);

        // integers
        private readonly SByteTypeMapping _tinyint          = new SByteTypeMapping("tinyint", DbType.SByte);
        private readonly ByteTypeMapping _utinyint          = new ByteTypeMapping("tinyint unsigned", DbType.Byte);
	    private readonly ShortTypeMapping _smallint         = new ShortTypeMapping("smallint", DbType.Int16);
	    private readonly UShortTypeMapping _usmallint       = new UShortTypeMapping("smallint unsigned", DbType.UInt16);
        private readonly IntTypeMapping _int                = new IntTypeMapping("int", DbType.Int32);
	    private readonly UIntTypeMapping _uint              = new UIntTypeMapping("int unsigned", DbType.UInt32);
	    private readonly LongTypeMapping _bigint            = new LongTypeMapping("bigint", DbType.Int64);
	    private readonly ULongTypeMapping _ubigint          = new ULongTypeMapping("bigint unsigned", DbType.UInt64);

	    // decimals
	    private readonly DecimalTypeMapping _decimal        = new DecimalTypeMapping("decimal(65, 30)", DbType.Decimal);
	    private readonly DoubleTypeMapping _double          = new DoubleTypeMapping("double", DbType.Double);
        private readonly FloatTypeMapping _float            = new FloatTypeMapping("float");

	    // binary
	    private readonly RelationalTypeMapping _binary           = new OracleByteArrayTypeMapping("binary", DbType.Binary);
        private readonly RelationalTypeMapping _varbinary        = new OracleByteArrayTypeMapping("varbinary", DbType.Binary);
	    private readonly OracleByteArrayTypeMapping _varbinary767 = new OracleByteArrayTypeMapping("varbinary(767)", DbType.Binary, 767);
	    private readonly RelationalTypeMapping _varbinarymax     = new OracleByteArrayTypeMapping("longblob", DbType.Binary);

	    // string
        private readonly OracleStringTypeMapping _char            = new OracleStringTypeMapping("char", DbType.AnsiStringFixedLength);
        private readonly OracleStringTypeMapping _varchar         = new OracleStringTypeMapping("varchar", DbType.AnsiString);
	    private readonly OracleStringTypeMapping _varchar127      = new OracleStringTypeMapping("varchar(127)", DbType.AnsiString, true, 127);
	    private readonly OracleStringTypeMapping _varcharmax      = new OracleStringTypeMapping("longtext", DbType.AnsiString);

        // DateTime
        private readonly OracleDateTimeTypeMapping _dateTime6 = new OracleDateTimeTypeMapping("datetime(6)", DbType.DateTime);
        private readonly OracleDateTypeMapping _date = new OracleDateTypeMapping("date", DbType.Date);
        private readonly OracleDateTimeOffsetTypeMapping _dateTimeOffset6  = new OracleDateTimeOffsetTypeMapping("datetime(6)", DbType.DateTime);
        private readonly OracleDateTimeOffsetTypeMapping _timeStamp6 = new OracleDateTimeOffsetTypeMapping("timestamp(6)", DbType.DateTime);
        private readonly TimeSpanTypeMapping _time6                       = new TimeSpanTypeMapping("time(6)", DbType.Time);

        // json
        private readonly RelationalTypeMapping _json         = new OracleJsonTypeMapping("json", DbType.String);

	    // row version
        private readonly RelationalTypeMapping _rowversion   = new OracleByteArrayTypeMapping("TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP", DbType.Binary);

        // guid
	    private readonly GuidTypeMapping _uniqueidentifier   = new GuidTypeMapping("char(36)", DbType.Guid);

        readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        public OracleTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    // boolean
                    { "bit", _bit },

                    // integers
                    { "tinyint", _tinyint },
                    { "tinyint unsigned", _utinyint },
                    { "smallint", _smallint },
                    { "smallint unsigned", _usmallint },
                    { "mediumint", _int },
                    { "mediumint unsigned", _uint },
                    { "int", _int },
                    { "int unsigned", _uint },
                    { "bigint", _bigint },
                    { "bigint unsigned", _ubigint },

                    // decimals
                    { "decimal", _decimal },
                    { "double", _double },
                    { "float", _float },

                    // binary
                    { "binary", _binary },
                    { "varbinary", _varbinary },
                    { "tinyblob", _varbinarymax },
                    { "blob", _varbinarymax },
                    { "mediumblob", _varbinarymax },
                    { "longblob", _varbinarymax },

                    // string
                    { "char", _char },
                    { "varchar", _varchar },
                    { "tinytext", _varcharmax },
                    { "text", _varcharmax },
                    { "mediumtext", _varcharmax },
                    { "longtext", _varcharmax },

                    // DateTime
                    { "datetime", _dateTime6 },
                    { "date", _date },
                    { "time", _time6 },
                    { "timestamp", _timeStamp6 },

                    // json
                    { "json", _json },

                    // guid
                    { "char(36)", _uniqueidentifier }
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
	                // boolean
	                { typeof(bool), _bit },

	                // integers
	                { typeof(short), _smallint },
	                { typeof(ushort), _usmallint },
	                { typeof(int), _int },
	                { typeof(uint), _uint },
	                { typeof(long), _bigint },
	                { typeof(ulong), _ubigint },

	                // decimals
	                { typeof(decimal), _decimal },
	                { typeof(float), _float },
	                { typeof(double), _double },

	                // byte / char
	                { typeof(sbyte), _tinyint },
	                { typeof(byte), _utinyint },
	                { typeof(char), _utinyint },

	                // DateTime
	                { typeof(DateTime), _dateTime6 },
	                { typeof(DateTimeOffset), _dateTimeOffset6 },
	                { typeof(TimeSpan), _time6 },

	                // json
	                { typeof(JsonObject<>), _json },

	                // guid
	                { typeof(Guid), _uniqueidentifier }
                };

            _disallowedMappings
                = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "binary",
                    "char",
                    "varbinary",
                    "varchar"
                };

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    8000,
                    _varbinarymax,
                    _varbinarymax,
                    _varbinary767,
                    _rowversion, size => new OracleByteArrayTypeMapping(
                        "varbinary(" + size + ")",
                        DbType.Binary,
                        size));

            StringMapper
                = new StringRelationalTypeMapper(
                    maxBoundedAnsiLength: 8000,
                    defaultAnsiMapping: _varcharmax,
                    unboundedAnsiMapping: _varcharmax,
                    keyAnsiMapping: _varchar127,
                    createBoundedAnsiMapping: size => new OracleStringTypeMapping(
                        "varchar(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size),
                    maxBoundedUnicodeLength: 8000,
                    defaultUnicodeMapping: _varcharmax,
                    unboundedUnicodeMapping: _varcharmax,
                    keyUnicodeMapping: _varchar127,
                    createBoundedUnicodeMapping: size => new OracleStringTypeMapping(
                        "varchar(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IStringRelationalTypeMapper StringMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void ValidateTypeName(string storeType)
        {
            if (_disallowedMappings.Contains(storeType))
            {
                throw new ArgumentException("UnqualifiedDataType" + storeType);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetColumnType(IProperty property) => property.Oracle().ColumnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            if (clrType.Name == typeof(JsonObject<>).Name)
                return _json;

            return clrType == typeof(string)
                ? _varcharmax
                : (clrType == typeof(byte[])
                    ? _varbinarymax
                    : base.FindMapping(clrType));
        }

        // Indexes in SQL Server have a max size of 900 bytes
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
