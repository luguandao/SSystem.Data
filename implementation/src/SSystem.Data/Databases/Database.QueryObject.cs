using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class Database
    {
        /// <summary>
        /// 数据查询，并把查询结果转化成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectSql"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryObject<T>(string selectSql) where T : class
        {
            if (string.IsNullOrEmpty(selectSql))
                throw new ArgumentNullException(nameof(selectSql));

            return QueryObject<T>(CreateCommand(selectSql));
        }

        /// <summary>
        /// 数据查询，并把查询结果转化成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryObject<T>(IDbCommand selectCommand) where T : class
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));
            var results = new List<T>();
            var type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            using (var reader = ExecuteReader(selectCommand))
            {
                var fieldNames = GetNames(reader);

                while (reader.Read())
                {
                    var item = (T)Compiler.DynamicMethodCompiler.CreateInstantiateObjectHandler(type)();
                    foreach (var prop in properties)
                    {
                        AssignValue(reader, fieldNames, prop, item);
                    }
                    results.Add(item);
                }
            }
            return results;
        }

        public async Task<IEnumerable<T>> QueryObjectAsync<T>(string selectSql) where T : class
        {
            if (string.IsNullOrEmpty(selectSql))
                throw new ArgumentNullException(nameof(selectSql));

            return await QueryObjectAsync<T>(CreateCommand(selectSql));
        }


        public async Task<IEnumerable<T>> QueryObjectAsync<T>(IDbCommand selectCommand) where T : class
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));
            var results = new List<T>();

            PropertyInfo[] properties;
            Type type = typeof(T);
            properties = type.GetProperties();

            using (var reader = await ExecuteReaderAsync(selectCommand))
            {
                var fieldNames = GetNames(reader);
                while (reader.Read())
                {
                    T item = Activator.CreateInstance<T>();
                    foreach (var prop in properties)
                    {
                        AssignValue(reader, fieldNames, prop, item);
                    }
                    results.Add(item);
                }
            }
            return results;
        }
    }
}
