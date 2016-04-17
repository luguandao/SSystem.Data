using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class Database
    {
        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(IDbCommand selectCommand)
        {
            var oValue = ExecuteScalar(selectCommand);
            return ConvertToT<T>(oValue);
        }

        public object ExecuteScalar(IDbCommand selectCommand)
        {
            if (selectCommand.Connection.State == ConnectionState.Closed)
            {
                selectCommand.Connection.Open();
            }

            return selectCommand.ExecuteScalar();
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectSql"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string selectSql)
        {
            return ExecuteScalar<T>(CreateCommand(selectSql));
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oval"></param>
        /// <returns></returns>
        public static T ConvertToT<T>(object oval)
        {
            T result = default(T);
            try
            {
                if (oval == null || oval == DBNull.Value) return result;
                result = (T)oval;
            }
            catch (Exception ex)
            {
                switch (typeof(T).Name.ToLower())
                {
                    case "string":
                        object tmp = Convert.ToString(oval);
                        result = (T)tmp;
                        break;
                    case "double":
                        result = (T)(object)Convert.ToDouble(oval);
                        break;
                    default:
                        MethodInfo[] methods = typeof(T).GetMethods();
                        MethodInfo meth = null;
                        foreach (MethodInfo item in methods)
                        {
                            if (item.IsPublic && item.Name == "Parse" && item.GetParameters().Length == 1)
                            {
                                meth = item;
                                break;
                            }
                        }
                        if (meth != null)//找到Parse方法
                        {
                            try
                            {
                                result = (T)meth.Invoke(result, new object[] { Convert.ToString(oval) });
                            }
                            catch
                            {
                                throw ex;
                            }
                        }
                        else
                        {
                            throw ex;
                        }
                        break;
                }
            }
            return result;
        }
    }
}
