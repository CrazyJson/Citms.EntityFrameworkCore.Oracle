using EFCoreExtend.Sql.Default;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EFCoreExtend
{
    /// <summary>
    /// 数据库参数化帮助类
    /// </summary>
    public static class DbParameterHelper
    {
        #region "私有方法"
        /// <summary>
        /// 转换字典类型为参数
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dictParams"></param>
        /// <param name="sqlProvider"></param>
        /// <returns></returns>
        private static IDataParameter[] DictionaryToDBParams(DatabaseFacade source,
           IEnumerable<KeyValuePair<string, object>> dictParams)
        {
            IDataParameter[] sqlParams = null;
            if (dictParams.Count() > 0)
            {
                //不使用Count()进行获取个数，而是参数传递count，然后扩展方法进行相应的扩展，从而提高性能
                sqlParams = new IDataParameter[dictParams.Count()];
                int i = 0;
                var sqlParamConverter =new SqlParamConverter();
                using (var command = source.GetDbConnection().CreateCommand())
                {
                    foreach (var p in dictParams)
                    {
                        var param = command.CreateParameter();
                        sqlParamConverter.InitParameter(param, p);
                        sqlParams[i++] = param;
                    }
                }
            }
            return sqlParams ?? new IDataParameter[0];
        }

        #endregion

        /// <summary>
        /// 转换object类型为KeyValuePair形式的对象
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, object>> ParseDynamicParam(object args)
        {
            if (args == null)
            {
                return new Dictionary<string, object>(0);
            }
            IEnumerable<KeyValuePair<string, object>> dict = null;
            if (args is IEnumerable<KeyValuePair<string, object>>)
            {
                dict = args as IEnumerable<KeyValuePair<string, object>>;
            }
            else if (args is Hashtable)
            {
                var ht = args as Hashtable;
                var dictTemp = new Dictionary<string, object>();
                foreach (string key in ht.Keys)
                {
                    dictTemp[key] = ht[key];
                }
                dict = dictTemp;
            }
            else
            {
                var dictTemp = new Dictionary<string, object>();
                PropertyInfo[] properties = args.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo pInfo in properties)
                {
                    object value = pInfo.GetValue(args);
                    dictTemp[pInfo.Name] = value;
                }
                dict = dictTemp;
            }
            return dict;
        }


        /// <summary>
        /// 转换动态类型为参数
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args">参数 支持new{a="1"} Hashtable Dictionary<string,object></param>
        /// <returns></returns>
        public static IDataParameter[] DynamicParamToDBParams(DatabaseFacade source,object args)
        {
            var dict = ParseDynamicParam(args);
            return DictionaryToDBParams(source, dict);
        }
    }
}
