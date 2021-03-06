﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

        public async Task<T> ExecuteScalarAsync<T>(IDbCommand selectCommand)
        {
            var oValue = await ExecuteScalarAsync(selectCommand);
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

        public async Task<object> ExecuteScalarAsync(IDbCommand selectCommand)
        {
            if (selectCommand.Connection.State == ConnectionState.Closed)
            {
                selectCommand.Connection.Open();
            }
            return await ((DbCommand)selectCommand).ExecuteScalarAsync();
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

        public static object ConvertTo(Type type, object oval)
        {
            if (type.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(type);
                if (defaultValue == oval)
                    return defaultValue;
            }
            else
            {
                if (oval == null)
                    return null;
            }

            object result = null;

            switch (type.Name.ToLower())
            {
                case "string":
                    object tmp = Convert.ToString(oval);
                    result = tmp;
                    break;
                case "double":
                    result = Convert.ToDouble(oval);
                    break;
                case "decimal":
                    result = Convert.ToDecimal(oval);
                    break;
                default:
                    MethodInfo[] methods = type.GetMethods();
                    MethodInfo meth = null;
                    foreach (MethodInfo item in methods)
                    {
                        if (item.IsPublic && ((item.Name == "Parse" && item.GetParameters().Length == 1) || item.Name == "get_Value"))
                        {
                            meth = item;
                            break;
                        }
                    }
                    if (meth != null)//找到Parse方法
                    {
                        if (meth.GetParameters().Length == 1)
                        {
                            result = meth.Invoke(oval, new object[] { Convert.ToString(oval) });
                        }
                        else if (meth.GetParameters().Length == 0)
                        {
                            result = meth.Invoke(oval, null);
                        }
                    }
                    break;
            }

            return result;
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
                    case "decimal":
                        result = (T)(object)Convert.ToDecimal(oval);
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
