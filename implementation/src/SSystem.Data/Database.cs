using System;
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
    public class Database : IDisposable
    {
        public IDbConnection Connection { get; }
        protected DbProviderFactory m_DbProviderFactory;
        protected ConnectionStringSettings m_ConnectionStringSettings;
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
                    break;
            }
        }

        public DatabaseType DatabaseType { get; }

        public IDbCommand CreateCommand(string commandText = null)
        {
            var commd = Connection.CreateCommand();
            commd.CommandText = commandText;
            return commd;
        }

        public int ExecuteNonQuery(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
            command.Connection = Connection;
            return command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string sql)
        {
            var icom = CreateCommand();
            icom.CommandText = sql;

            return ExecuteNonQuery(icom);
        }

        public int ExecuteNonQuery(DataTable table)
        {
            var comm = CreateCommand();
            comm.CommandText = "select * from " + table.TableName;
            IDataAdapter adapt = CreateDbDataAdapter(comm, DbCommandType.AllCommand);

            string oldName = table.TableName;
            table.TableName = "Table";
            int n = adapt.Update(table.DataSet);
            table.TableName = oldName;
            return n;
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

        private void AssignValue<T>(IDataReader reader, PropertyInfo propertyInfo, T target)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var index = reader.GetOrdinal(propertyInfo.Name);
            var value = reader.GetValue(index);
            if (value == DBNull.Value)
            {
                value = null;
            }

            if (index > -1)
            {
                propertyInfo.SetValue(target, value);
            }
        }

        public IDataReader CreateDataReader(IDbCommand selectCommand, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));

            if (selectCommand.Connection == null)
            {
                selectCommand.Connection = Connection;
            }
            if (selectCommand.Connection.State == ConnectionState.Closed)
            {
                selectCommand.Connection.Open();
            }
            return selectCommand.ExecuteReader(behavior);
        }

        public IDataReader CreateDataReader(string selectSql, CommandBehavior behavior = CommandBehavior.CloseConnection) =>
            CreateDataReader(CreateCommand(selectSql), behavior);

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

        private IDbConnection CreateConnection(string connectionString)
        {
            if (m_DbProviderFactory == null)
            {
                m_DbProviderFactory = CreateDbProviderFactory();
            }
            IDbConnection icon = m_DbProviderFactory.CreateConnection();
            icon.ConnectionString = connectionString;
            return icon;
        }

        private DbProviderFactory CreateDbProviderFactory() => DbProviderFactories.GetFactory(m_ConnectionStringSettings.ProviderName);
    }
}
