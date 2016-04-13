using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class Database
    {
        public IDataReader CreateDataReader(IDbCommand selectCommand, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));

            if (selectCommand.Connection == null)
            {
                selectCommand.Connection = CurrentConnection;
            }
            if (selectCommand.Connection.State == ConnectionState.Closed)
            {
                selectCommand.Connection.Open();
            }
            return selectCommand.ExecuteReader(behavior);
        }

        public IDataReader CreateDataReader(string selectSql, CommandBehavior behavior = CommandBehavior.CloseConnection) =>
            CreateDataReader(CreateCommand(selectSql), behavior);
    }
}
