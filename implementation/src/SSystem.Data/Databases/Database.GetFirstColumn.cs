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
        public IList<string> GetFirstColumn(IDbCommand selectCommand)
        {
            if (selectCommand == null)
                throw new ArgumentNullException(nameof(selectCommand));

            var columnValue = new List<string>();
            using (var reader = ExecuteReader(selectCommand))
            {
                while (reader.Read())
                {
                    object oValue = reader[0];
                    switch (oValue.GetType().Name.ToLower())
                    {
                        case "byte[]":
                            byte[] btmp = reader[0] as byte[];
                            StringBuilder sbb = new StringBuilder();
                            sbb.Append("0x");
                            foreach (byte b in btmp)
                            {
                                sbb.Append(b.ToString("D2"));
                            }
                            columnValue.Add(sbb.ToString());
                            break;
                        default:
                            columnValue.Add(Convert.ToString(reader[0]));
                            break;
                    }
                }
                reader.Close();
            }
            return columnValue;
        }

        public IList<string> GetFirstColumn(string selectSql)
        {
            return GetFirstColumn(CreateCommand(selectSql));
        }
    }
}
