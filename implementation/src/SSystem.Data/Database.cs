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
        public IDbConnection Connection { get; }
        protected DbProviderFactory m_DbProviderFactory;
        protected ConnectionStringSettings m_ConnectionStringSettings;
        /// <summary>
        /// 等待命令所需时间，以秒为单位
        /// </summary>
        public static int DefaultCommandTimeoutBySeconds = 30;
        public string TagName { get; private set; }
        public Database(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(name);
            m_ConnectionStringSettings = ConfigurationManager.ConnectionStrings[name];

            if (m_ConnectionStringSettings == null)
            {
                throw new SettingsPropertyNotFoundException("cannot found connection string");
            }
            Connection = CreateConnection(m_ConnectionStringSettings.ConnectionString);
            if (Connection == null)
                throw new Exception("cannot initial connection");

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
                default:
                    throw new NotImplementedException(connTypeName);
            }
        }

        public DatabaseType DatabaseType { get; }

        public IDbCommand CreateCommand(string commandText = null) => CreateCommand(commandText, null);
        public IDbCommand CreateCommand(string commandText, IDictionary parameters)
        {
            var commd = Connection.CreateCommand();
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

        public DataSet Query(IDbCommand selectCommand, bool allowSchema = true, string tableName = "table1")
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));

            IDbDataAdapter adapt = CreateDbDataAdapter(selectCommand, DbCommandType.SelectCommand);
            var result = new DataSet();
            if (allowSchema)
            {
                adapt.FillSchema(result, SchemaType.Source);
            }
            adapt.Fill(result);

            result.Tables[0].TableName = tableName;
            return result;
        }

        public DataSet Query(string selectSql, bool allowSchema = true, string tableName = "table1")
        {
            var selectCommand = CreateCommand();
            selectCommand.CommandText = selectSql;
            return Query(selectCommand, allowSchema, tableName);
        }

        public T GetObject<T>(string selectSql) where T : class
        {
            if (string.IsNullOrEmpty(selectSql))
                throw new ArgumentNullException(nameof(selectSql));

            T result = Activator.CreateInstance<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            using (var reader = CreateDataReader(selectSql))
            {
                if (reader.Read())
                {
                    foreach (var prop in properties)
                    {
                        AssignValue<T>(reader, prop, result);
                    }
                }
            }
            return result;
        }

        public IEnumerable<T> GetObjectList<T>(string selectSql) where T : class
        {
            if (string.IsNullOrEmpty(selectSql))
                throw new ArgumentNullException(nameof(selectSql));

            List<T> results = new List<T>();

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            using (var reader = CreateDataReader(selectSql))
            {
                while (reader.Read())
                {
                    T item = Activator.CreateInstance<T>();
                    foreach (var prop in properties)
                    {

                        AssignValue(reader, prop, item);

                    }
                    results.Add(item);
                }
            }
            return results;
        }

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


        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }


    }
}
