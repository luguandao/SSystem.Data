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

        public int ExecuteNonQuery(string commandText)
        {
            var icom = CreateCommand(commandText);
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
    }
}
