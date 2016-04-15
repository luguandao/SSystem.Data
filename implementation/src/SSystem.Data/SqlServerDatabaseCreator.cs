using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    /// <summary>
    /// 数据库生成器
    /// </summary>
    public class SqlServerDatabaseCreator
    {
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        public string DbName { get; }
        /// <summary>
        /// 获取或设置数据库文件目录
        /// </summary>
        public string DbFileDirectory { get; set; }
        /// <summary>
        /// 获取或设置文件最大值，默认：100Mb
        /// </summary>
        public string MaxSize { get; set; } = "100Mb";
        /// <summary>
        /// 
        /// </summary>
        public string FileGrowth { get; set; } = "15%";
        public string DataFileInitialSize { get; set; } = "5Mb";
        public string LogFileInitialSize { get; set; } = "2Mb";
        public string LogFileGrowth { get; set; } = "1Mb";
        public SqlServerDatabaseCreator(string dbName,string dbFileDirectory)
        {
            if (string.IsNullOrEmpty(dbName))
                throw new ArgumentNullException(nameof(dbName));
            if (string.IsNullOrEmpty(dbFileDirectory))
                throw new ArgumentNullException(nameof(dbFileDirectory));

            DbName = dbName;
            DbFileDirectory = dbFileDirectory;
        }

        public void Create(string connectionStringName)
        {
            string sqlScript = string.Format(@"create DATABASE {0}
on primary
(
name='{0}_data',
filename='{1}',
size={2},
maxsize={3},
filegrowth={4}
)
log on
(
name='{0}_log',
filename='{5}',
size={6},
filegrowth={7}
)
", DbName, Path.Combine(DbFileDirectory, string.Format("{0}_data.mdf", DbName)), DataFileInitialSize, MaxSize, FileGrowth,
Path.Combine(DbFileDirectory, string.Format("{0}_log.ldf", DbName)), LogFileInitialSize, LogFileGrowth);

            using (var context = new Database(connectionStringName))
            {
                context.ExecuteNonQuery(sqlScript);
            }
        }
    }
}
