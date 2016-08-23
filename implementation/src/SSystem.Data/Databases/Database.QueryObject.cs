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
            CommandBehavior behavior = CommandBehavior.CloseConnection;
            if (Transaction != null && Connection.State != ConnectionState.Closed)
            {
                behavior = CommandBehavior.Default;
            }

            using (var reader = ExecuteReader(selectCommand, behavior))
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

        /// <summary>
        /// 执行查询语句，并且以对象形式返回
        /// 此接口需要传入两条语句，第一条语句是查询返回对象，第二条语句返回int型，中间用英文的分号隔开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public QueryObjectResult<T> QueryObjectWithCount<T>(IDbCommand selectCommand)
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));
            if (!selectCommand.CommandText.Contains(";"))
            {
                throw new ArgumentException("应该包含返回count的sql语句，并且放在查询语句的后面，用英文的分号隔开");
            }
            var result = new QueryObjectResult<T>();
            var list = new List<T>();
            var type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            CommandBehavior behavior = CommandBehavior.CloseConnection;
            if (Transaction != null && Connection.State != ConnectionState.Closed)
            {
                behavior = CommandBehavior.Default;
            }

            using (var reader = ExecuteReader(selectCommand, behavior))
            {
                var fieldNames = GetNames(reader);

                while (reader.Read())
                {
                    var item = (T)Compiler.DynamicMethodCompiler.CreateInstantiateObjectHandler(type)();
                    foreach (var prop in properties)
                    {
                        AssignValue(reader, fieldNames, prop, item);
                    }
                    list.Add(item);
                }
                result.Objects = list;
                if (reader.NextResult() && reader.Read())
                {
                    result.Count = Convert.ToInt32(reader[0]);
                }

            }
            return result;
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
            CommandBehavior behavior = CommandBehavior.CloseConnection;
            if (Transaction != null && Connection.State != ConnectionState.Closed)
            {
                behavior = CommandBehavior.Default;
            }

            using (var reader = await ExecuteReaderAsync(selectCommand, behavior))
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

        /// <summary>
        /// 执行查询语句，并且以对象形式返回
        /// 此接口需要传入两条语句，第一条语句是查询返回对象，第二条语句返回int型，中间用英文的分号隔开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public async Task<QueryObjectResult<T>> QueryObjectWithCountAsync<T>(IDbCommand selectCommand)
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));
            if (!selectCommand.CommandText.Contains(";"))
            {
                throw new ArgumentException("应该包含返回count的sql语句，并且放在查询语句的后面，用英文的分号隔开");
            }
            QueryObjectResult<T> result = new QueryObjectResult<T>();
            var list = new List<T>();
            var type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            CommandBehavior behavior = CommandBehavior.CloseConnection;
            if (Transaction != null && Connection.State != ConnectionState.Closed)
            {
                behavior = CommandBehavior.Default;
            }

            using (var reader = await ExecuteReaderAsync(selectCommand, behavior))
            {
                var fieldNames = GetNames(reader);

                while (reader.Read())
                {
                    var item = (T)Compiler.DynamicMethodCompiler.CreateInstantiateObjectHandler(type)();
                    foreach (var prop in properties)
                    {
                        AssignValue(reader, fieldNames, prop, item);
                    }
                    list.Add(item);
                }
                result.Objects = list;
                if (reader.NextResult() && reader.Read())
                {
                    result.Count = Convert.ToInt32(reader[0]);
                }
            }
            return result;
        }
    }
}
