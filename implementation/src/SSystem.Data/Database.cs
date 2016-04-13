using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    /// <summary>
    /// 数据操作类
    /// </summary>
    public partial class Database : IDisposable
    {
        /// <summary>
        /// 获取当前数据库连接
        /// </summary>
        public IDbConnection CurrentConnection { get; }
        private DbProviderFactory m_DbProviderFactory;
        private string m_ProviderName;
        /// <summary>
        /// 等待命令所需时间，以秒为单位
        /// </summary>
        public static int DefaultCommandTimeoutBySeconds = 30;
        /// <summary>
        /// 获取此数据库类型的前缀标记符号
        /// </summary>
        public string TagName { get; private set; }
        /// <summary>
        /// 数据操作类的构造函数
        /// </summary>
        /// <param name="name">配置数据库连接名称</param>
        public Database(string name) : this(ConfigurationManager.ConnectionStrings[name].ConnectionString,
            ConfigurationManager.ConnectionStrings[name].ProviderName)
        {
        }

        /// <summary>
        /// 数据操作类的构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        public Database(string connectionString, string providerName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException(nameof(providerName));

            m_ProviderName = providerName;
            CurrentConnection = CreateConnection(connectionString);
            if (CurrentConnection == null)
                throw new Exception("cannot initial connection");

            var connTypeName = CurrentConnection.GetType().Name.ToLower();
            switch (connTypeName)
            {
                case "sqlconnection":
                    DatabaseType = DatabaseType.SqlServer;
                    TagName = "@";
                    break;
                case "oralceconnection":
                    DatabaseType = DatabaseType.Oracle;
                    TagName = ":";
                    break;
                default:
                    throw new NotImplementedException(connTypeName);
            }
        }

        /// <summary>
        /// 获取当前数据库类型
        /// </summary>
        public DatabaseType DatabaseType { get; }

        /// <summary>
        /// 生成Command
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public IDbCommand CreateCommand(string commandText = null) => CreateCommand(commandText, null);

        /// <summary>
        /// 生成Command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDbCommand CreateCommand(string commandText, IDictionary parameters)
        {
            var commd = CurrentConnection.CreateCommand();
            commd.CommandText = ReplaceProfixTag(commandText);
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;
            if (parameters != null && parameters.Count > 0)
            {
                var er = parameters.GetEnumerator();
                while (er.MoveNext())
                {
                    commd.Parameters.Add(CreateIDataParameter(TagName + er.Key, er.Value, ParameterDirection.Input));
                }
            }
            return commd;
        }

        
        /// <summary>
        /// 数据查询，并把查询结果转化成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectSql"></param>
        /// <returns></returns>
        public IEnumerable<T> GetObjectList<T>(string selectSql) where T : class
        {
            if (string.IsNullOrEmpty(selectSql))
                throw new ArgumentNullException(nameof(selectSql));

            return GetObjectList<T>(CreateCommand(selectSql));
        }

        /// <summary>
        /// 数据查询，并把查询结果转化成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public IEnumerable<T> GetObjectList<T>(IDbCommand selectCommand) where T : class
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));
            List<T> results = new List<T>();

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            using (var reader = ExecuteReader(selectCommand))
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
        /// 生成DbDataAdapter
        /// </summary>
        /// <param name="selectCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IDbDataAdapter CreateDbDataAdapter(IDbCommand selectCommand, DbCommandType type)
        {
            var adapt = m_DbProviderFactory.CreateDataAdapter();
            adapt.SelectCommand = selectCommand as DbCommand;
            DbCommandBuilder builder = m_DbProviderFactory.CreateCommandBuilder();
            builder.DataAdapter = adapt;

            adapt.InsertCommand = ((type & DbCommandType.InsertCommand) == DbCommandType.InsertCommand) ?
            builder.GetInsertCommand() : adapt.InsertCommand;

            adapt.UpdateCommand = ((type & DbCommandType.UpdateCommand) == DbCommandType.UpdateCommand) ?
                builder.GetUpdateCommand() : adapt.UpdateCommand;

            adapt.DeleteCommand = ((type & DbCommandType.DeleteCommand) == DbCommandType.DeleteCommand) ?
                builder.GetDeleteCommand() : adapt.DeleteCommand;

            adapt.SelectCommand = ((type & DbCommandType.SelectCommand) == DbCommandType.SelectCommand) ?
                adapt.SelectCommand : null;
            return adapt;
        }


        /// <summary>
        /// 释放数据库连接
        /// </summary>
        public void Dispose()
        {
            CurrentConnection.Close();
            CurrentConnection.Dispose();
        }


    }
}
