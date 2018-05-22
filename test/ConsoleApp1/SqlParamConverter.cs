using System;
using System.Collections.Generic;
using System.Data;

namespace EFCoreExtend.Sql.Default
{
    /// <summary>
    /// 默认SQL参数转换器
    /// </summary>
    public class SqlParamConverter
    {
        /// <summary>
        /// 将自定义KeyValuePair转换成DbParameter
        /// </summary>
        /// <param name="parameter">数据库DbParameter</param>
        /// <param name="param">自定义参数</param>
        /// <returns></returns>
        public void InitParameter(IDataParameter parameter, KeyValuePair<string, object> param)
        {
            parameter.ParameterName = param.Key;
            parameter.Value = param.Value ?? DBNull.Value;
        }
    }
}
