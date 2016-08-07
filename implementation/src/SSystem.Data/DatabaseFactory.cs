using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SSystem.Data
{
    public partial class DatabaseFactory
    {
        private static Dictionary<DatabaseType, DbProviderFactory> _ProviderFactories = new Dictionary<DatabaseType, DbProviderFactory>();
        private static Dictionary<string, DatabaseTypeConnectionItem> _DbTypeConnectionStringCached = new Dictionary<string, DatabaseTypeConnectionItem>();
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
            DatabaseType type;
            if (_DbTypeConnectionStringCached.ContainsKey(connectionStringName))
            {
                type = _DbTypeConnectionStringCached[connectionStringName].DbType;
            }
            else if (_ProviderFactories.Any())
            {
                type = _ProviderFactories.First().Key;
            }
            else
                throw new Exception("请调用AddProviderFactory初始化数据库类型");

            return Create(type, connectionStringName, isTransaction);
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

        /// <summary>
        /// 往内存中初始化数据库配置，优先读取缓存，如果找不到，才会寻找app.config中的配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="connectionString"></param>
        public static void AddConnectionString(string name, DatabaseType type, string connectionString)
        {
            if (!_DbTypeConnectionStringCached.ContainsKey(name))
            {
                _DbTypeConnectionStringCached.Add(name, new DatabaseTypeConnectionItem
                {
                    ConnectionString = connectionString,
                    DbType = type
                });
            }
        }

        private static string GetConnectionString(string connectionStringName)
        {
            if (_DbTypeConnectionStringCached.ContainsKey(connectionStringName))
                return _DbTypeConnectionStringCached[connectionStringName].ConnectionString;

            var setting = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];
            if (setting != null)
                return setting.ConnectionString;
            throw new NotSupportedException($"没有找到数据库配置:{connectionStringName}");
        }

        private class DatabaseTypeConnectionItem
        {
            public DatabaseType DbType { get; set; }
            public string ConnectionString { get; set; }
        }
    }

}
