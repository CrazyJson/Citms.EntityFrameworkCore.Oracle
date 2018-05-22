using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using EFCoreExtend;

namespace Microsoft.EntityFrameworkCore
{
    public static class SqlExtensions
    {  
        /// <summary>
        /// 执行命令,将结果集转换为实体集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="databaseFacade">数据库对象</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="args">sql的参数</param>
        /// <returns>数据集合</returns>
        public static List<T> Query<T>(this DatabaseFacade databaseFacade, string sql, object args = null)
            where T : class
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();
            using (concurrencyDetector.EnterCriticalSection())
            {
                var infrastructure = databaseFacade as IInfrastructure<IServiceProvider>;
                var rawSqlCommand = infrastructure.GetService<IRawSqlCommandBuilder>()
                    .Build(sql, DbParameterHelper.DynamicParamToDBParams(databaseFacade, args));
                using (var reader = rawSqlCommand
                    .RelationalCommand
                    .ExecuteReader(
                        infrastructure.GetService<IRelationalConnection>(),
                        rawSqlCommand.ParameterValues))
                {
                    using (reader.DbDataReader)
                    {
                        return ToList<T>(reader.DbDataReader);
                    }
                }
            }
        }

        /// <summary>
        /// 执行命令,返回第一行,第一列的值,并将结果转换为T类型
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="databaseFacade">数据库对象</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="args">sql的参数</param>
        /// <returns>结果集的第一行,第一列</returns>
        public static T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, object args = null)
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();
            using (concurrencyDetector.EnterCriticalSection())
            {
                var infrastructure = databaseFacade as IInfrastructure<IServiceProvider>;
                var rawSqlCommand = infrastructure.GetService<IRawSqlCommandBuilder>()
                    .Build(sql, DbParameterHelper.DynamicParamToDBParams(databaseFacade, args));
                var result = rawSqlCommand
                    .RelationalCommand
                    .ExecuteScalar(
                        infrastructure.GetService<IRelationalConnection>(),
                        rawSqlCommand.ParameterValues);
                return ConvertScalar<T>(result);
            }
        }

        /// <summary>
        /// 执行命令,将第一列的值填充到类型为T的行集合中
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="databaseFacade">数据库对象</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="args">sql的参数</param>
        /// <returns>结果集的第一列集合</returns>
        public static List<T> FillScalarList<T>(this DatabaseFacade databaseFacade, string sql, object args = null)
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();
            using (concurrencyDetector.EnterCriticalSection())
            {
                var infrastructure = databaseFacade as IInfrastructure<IServiceProvider>;
                var rawSqlCommand = infrastructure.GetService<IRawSqlCommandBuilder>()
                    .Build(sql, DbParameterHelper.DynamicParamToDBParams(databaseFacade, args));
                using (var reader = rawSqlCommand
                    .RelationalCommand
                    .ExecuteReader(
                        infrastructure.GetService<IRelationalConnection>(),
                        rawSqlCommand.ParameterValues))
                {
                    List<T> list = new List<T>();
                    using (reader.DbDataReader)
                    {
                        while (reader.DbDataReader.Read())
                        {
                            list.Add(ConvertScalar<T>(reader.DbDataReader[0]));
                        }
                        return list;
                    }
                }
            }
        }

        /// <summary>
        /// 执行命令,将结果集第一行转换为实体
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="databaseFacade">数据库对象</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="args">sql的参数</param>
        /// <returns></returns>
        public static T ToSingle<T>(this DatabaseFacade databaseFacade, string sql, object args = null) where T : class
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();
            using (concurrencyDetector.EnterCriticalSection())
            {
                var infrastructure = databaseFacade as IInfrastructure<IServiceProvider>;
                var rawSqlCommand = infrastructure.GetService<IRawSqlCommandBuilder>()
                    .Build(sql, DbParameterHelper.DynamicParamToDBParams(databaseFacade, args));
                using (var reader = rawSqlCommand
                    .RelationalCommand
                    .ExecuteReader(
                        infrastructure.GetService<IRelationalConnection>(),
                        rawSqlCommand.ParameterValues))
                {
                    using (reader.DbDataReader)
                    {
                        return ToSingle<T>(reader.DbDataReader);
                    }
                }
            }
        }

        /// <summary>
        /// 执行命令,并返回影响函数
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="databaseFacade">数据库对象</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="args">sql的参数</param>
        /// <returns>影响行数</returns>
        public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, object args = null)
        {
            return databaseFacade.ExecuteSqlCommand(sql, DbParameterHelper.DynamicParamToDBParams(databaseFacade, args));
        }



        #region "私有方法"

        internal static T ConvertScalar<T>(object obj)
        {
            if (obj == null || DBNull.Value.Equals(obj))
                return default(T);

            if (obj is T)
                return (T)obj;

            Type targetType = typeof(T);

            if (targetType == typeof(object))
                return (T)obj;

            return (T)Convert.ChangeType(obj, targetType);
        }

        private static string[] GetColumnNames(DbDataReader reader)
        {
            int count = reader.FieldCount;
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = reader.GetName(i);
            }
            return names;
        }

        private static List<T> ToList<T>(DbDataReader reader) where T : class
        {
            Type type = typeof(T);
            var dict = TypeDescription.GetTypeDiscription(type);

            List<T> list = new List<T>();
            string[] names = GetColumnNames(reader);
            while (reader.Read())
            {
                T obj = Activator.CreateInstance(type) as T;
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];
                    if (dict.TryGetValue(name, out ExPropertyMap info))
                    {
                        object val = reader.GetValue(i);
                        if (val != null && DBNull.Value.Equals(val) == false)
                        {
                            info.PropertyInfo.SetValue(obj, ConvertExt.Convert(val, info.PropertyInfo.PropertyType));
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        private static T ToSingle<T>(DbDataReader reader) where T : class
        {
            Type type = typeof(T);

            var dict = TypeDescription.GetTypeDiscription(type);

            if (reader.Read())
            {
                string[] names = GetColumnNames(reader);

                T obj = Activator.CreateInstance(type) as T;
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];

                    if (dict.TryGetValue(name, out ExPropertyMap info))
                    {
                        object val = reader.GetValue(i);
                        if (val != null && DBNull.Value.Equals(val) == false)
                        {
                            info.PropertyInfo.SetValue(obj, ConvertExt.Convert(val, info.PropertyInfo.PropertyType));
                        }
                    }
                }
                return obj;
            }
            else
            {
                return default(T);
            }
        }
        #endregion
    }

    #region "internal class"

    /// <summary>
    /// 类型缓存
    /// </summary>
    internal class TypeDescription
    {
        private static ConcurrentDictionary<string, Dictionary<string, ExPropertyMap>> s_typeInfoDict = new ConcurrentDictionary<string, Dictionary<string, ExPropertyMap>>();

        private static BindingFlags s_flag = BindingFlags.Instance | BindingFlags.Public;

        public static Dictionary<string, ExPropertyMap> GetTypeDiscription(Type type)
        {
            if (!s_typeInfoDict.TryGetValue(type.FullName, out Dictionary<string, ExPropertyMap> description) || description == null)
            {
                PropertyInfo[] properties = type.GetProperties(s_flag);
                int length = properties.Length;
                description = new Dictionary<string, ExPropertyMap>(length, StringComparer.OrdinalIgnoreCase);

                foreach (PropertyInfo prop in properties)
                {
                    ExPropertyMap info = null;
                    var attrColumn = prop.GetCustomAttribute<ColumnAttribute>();
                    info = new ExPropertyMap { ColumnName = attrColumn != null ? attrColumn.Name : prop.Name, PropertyInfo = prop };
                    description[info.ColumnName] = info;
                }
                s_typeInfoDict[type.FullName] = description;
            }

            return description;
        }
    }

    internal class ExPropertyMap
    {
        public string ColumnName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }

    internal class ConvertExt
    {
        public static object Convert(object value, Type targetType)
        {
            if (value == null)
                return null;

            if (targetType == typeof(string))
                return value.ToString();

            Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (value.GetType() == type)
            {
                return value;
            }

            if (type == typeof(Guid) && value.GetType() == typeof(string))
            {
                return new Guid(value.ToString());
            }

            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            return System.Convert.ChangeType(value, type);
        }
    }
    #endregion
}
