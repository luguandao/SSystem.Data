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
        /// <summary>
        /// 数据查询
        /// </summary>
        /// <param name="selectCommand"></param>
        /// <param name="allowSchema"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataSet Query(IDbCommand selectCommand, bool allowSchema = true, string tableName = "table1")
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));

            var adapt = CreateDbDataAdapter(selectCommand, DbCommandType.SelectCommand);
            var result = new DataSet();
            if (allowSchema)
            {
                adapt.FillSchema(result, SchemaType.Source);
            }
            adapt.Fill(result);

            result.Tables[0].TableName = tableName;
            return result;
        }

        /// <summary>
        /// 数据查询
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="allowSchema"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataSet Query(string selectSql, bool allowSchema = true, string tableName = "table1")
        {
            var selectCommand = CreateCommand();
            selectCommand.CommandText = selectSql;
            return Query(selectCommand, allowSchema, tableName);
        }

    }
}
