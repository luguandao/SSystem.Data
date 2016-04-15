using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class Database
    {
        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (CurrentConnection.State == ConnectionState.Closed)
            {
                CurrentConnection.Open();
            }
            command.Connection = CurrentConnection;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text)
        {
            var icom = CreateCommand(commandText);
            icom.CommandType = commandType;
            return ExecuteNonQuery(icom);
        }

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 异步执行非查询语句
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            DbCommand comm = (DbCommand)command;
            if (CurrentConnection.State == ConnectionState.Closed)
            {
                CurrentConnection.Open();
            }
            comm.Connection = CurrentConnection as DbConnection;
            return await comm.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 异步执行非查询语句
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.Text)
        {
            var comm = CreateCommand(commandText);
            comm.CommandType = commandType;
            return await ExecuteNonQueryAsync(comm);
        }
    }
}
