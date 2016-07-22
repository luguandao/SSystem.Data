using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class DatabaseFactory
    {
        private static Dictionary<DatabaseType, DbProviderFactory> _ProviderFactories = new Dictionary<DatabaseType, DbProviderFactory>();
        public static void AddProviderFactory(DbProviderFactory factory, DatabaseType dbtype)
        {
            if (_ProviderFactories.ContainsKey(dbtype))
                throw new Exception("重复初始化");

            _ProviderFactories.Add(dbtype, factory);
        }
        public static Database Create(DatabaseType type, string connectionStringName, bool isTransaction = false)
        {
            if (!_ProviderFactories.ContainsKey(type))
                throw new Exception("没有初始化数据库类型：" + type);

            var conn = _ProviderFactories[type].CreateConnection();
            conn.ConnectionString = GetConnectionString(connectionStringName);

            var database = new Database(conn, isTransaction);
            database.DbProviderFactory = _ProviderFactories[type];
            return database;
        }

        public static Database Create(string connectionStringName, bool isTransaction = false)
        {
            if (_ProviderFactories.Count == 0)
                throw new Exception("请调用AddProviderFactory初始化数据库类型");
            return Create(_ProviderFactories.First().Key, connectionStringName, isTransaction);
        }

        public static Database CreateByConnectionString(DatabaseType type, string connectionString, bool isTransaction = false)
        {
            if (!_ProviderFactories.ContainsKey(type))
                throw new Exception("没有初始化数据库类型：" + type);

            var conn = _ProviderFactories[type].CreateConnection();
            conn.ConnectionString = connectionString;

            var database = new Database(conn, isTransaction);
            database.DbProviderFactory = _ProviderFactories[type];
            return database;
        }

        public static Database CreateByConnectionString(string connectionString, bool isTransaction = false)
        {
            if (_ProviderFactories.Count == 0)
                throw new Exception("请调用AddProviderFactory初始化数据库类型");
            return CreateByConnectionString(_ProviderFactories.First().Key, connectionString, isTransaction);
        }

        private static string GetConnectionString(string connectionStringName)
        {
            var setting = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];
            if (setting != null)
                return setting.ConnectionString;
            return connectionStringName;
        }
    }
}
