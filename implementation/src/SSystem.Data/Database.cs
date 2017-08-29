using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
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
        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; private set; }
        public int TimeoutOfCaching { get; set; } = 1440;
        internal DbProviderFactory DbProviderFactory;
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
        internal Database(string name, bool isTransaction = false) : this(ConfigurationManager.ConnectionStrings[name].ConnectionString,
            ConfigurationManager.ConnectionStrings[name].ProviderName, isTransaction)
        {
        }

        /// <summary>
        /// 数据操作类的构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <param name="isTransaction"></param>
        internal Database(string connectionString, string providerName, bool isTransaction = false)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException(nameof(providerName));

            m_ProviderName = providerName;
            Connection = CreateConnection(connectionString);
            InitByConnection(Connection, isTransaction);
        }

        internal Database(IDbConnection icon, bool isTransaction)
        {
            InitByConnection(icon, isTransaction);
        }

        private void InitByConnection(IDbConnection icon, bool isTransaction)
        {
            Connection = icon;
            if (Connection == null)
                throw new Exception("cannot initial connection");

            if (isTransaction)
            {
                Connection.Open();
                Transaction = Connection.BeginTransaction();
            }

            var connTypeName = Connection.GetType().Name.ToLower();
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
                case "sqliteconnection":
                    DatabaseType = DatabaseType.Sqlite;
                    TagName = "@";
                    break;
                case "mysqlconnection":
                    DatabaseType = DatabaseType.MySql;
                    TagName = "?";
                    break;
                default:
                    throw new NotImplementedException(connTypeName);
            }
        }

        /// <summary>
        /// 获取当前数据库类型
        /// </summary>
        public DatabaseType DatabaseType { get; private set; }

        /// <summary>
        /// 生成Command
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>

        /// <summary>
        /// 生成DbDataAdapter
        /// </summary>
        /// <param name="selectCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IDbDataAdapter CreateDbDataAdapter(IDbCommand selectCommand, DbCommandType type)
        {
            var adapt = DbProviderFactory.CreateDataAdapter();
            adapt.SelectCommand = selectCommand as DbCommand;
            DbCommandBuilder builder = DbProviderFactory.CreateCommandBuilder();
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

        public void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            if (Transaction == null)
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }
                Transaction = Connection.BeginTransaction(level);
            }
        }

        /// <summary>
        /// 释放数据库连接
        /// </summary>
        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
            }

            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }

        /// <summary>
        /// 获取实体类所对应的表名
        /// </summary>
        /// <returns></returns>
        public string GetTableName<T>() => DBHelper.GetTableName<T>();
    }
}
