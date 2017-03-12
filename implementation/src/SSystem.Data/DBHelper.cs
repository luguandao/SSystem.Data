using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    /// <summary>
    /// 操作辅助工具类
    /// </summary>
    public static class DBHelper
    {
        /// <summary>
        /// 获取实体类所对应的表名
        /// </summary>
        /// <returns></returns>
        public static string GetTableName<T>()
        {
            var type = typeof(T);
            var customType = typeof(TableAttribute);
            var attrs = type.GetCustomAttributes(customType, true);
            var selected = attrs.FirstOrDefault(a => a.GetType() == customType);
            if (selected == null)
                return type.Name;
            return ((TableAttribute)selected).Name;
        }
    }
}
