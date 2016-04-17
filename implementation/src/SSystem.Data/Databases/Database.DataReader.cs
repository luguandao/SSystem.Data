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
        /// 生成DataReader
        /// </summary>
        /// <param name="selectCommand"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(IDbCommand selectCommand, CommandBehavior behavior = CommandBehavior.CloseConnection)
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

        /// <summary>
        /// 生成DataReader
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string selectSql, CommandBehavior behavior = CommandBehavior.CloseConnection) =>
            ExecuteReader(CreateCommand(selectSql), behavior);

        /// <summary>
        /// 生成DataReader
        /// </summary>
        /// <param name="selectCommand"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand selectCommand, CommandBehavior behavior = CommandBehavior.CloseConnection)
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
            return await ((DbCommand)selectCommand).ExecuteReaderAsync(behavior);
        }

        /// <summary>
        /// 生成DataReader
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="commandType"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public async Task<IDataReader> ExecuteReaderAsync(string selectSql, CommandType commandType = CommandType.Text, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            var comm = CreateCommand(selectSql);
            comm.CommandType = commandType;
            return await ExecuteReaderAsync(comm, behavior);
        }
    }
}
