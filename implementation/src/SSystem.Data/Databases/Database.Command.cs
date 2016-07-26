using SSystem.Data.Compiler;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class Database
    {
        private readonly static string _Tablename = "_tablename";
        public IDbCommand CreateCommand(string commandText = null) => CreateCommand(commandText, null);

        /// <summary>
        /// 生成Command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDbCommand CreateCommand(string commandText, IDictionary parameters)
        {
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandText = ReplaceProfixTag(commandText);
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;
            if (parameters != null && parameters.Count > 0)
            {
                var er = parameters.GetEnumerator();
                while (er.MoveNext())
                {
                    commd.Parameters.Add(CreateIDataParameter(TagName + er.Key, er.Value == null ? DBNull.Value : er.Value, ParameterDirection.Input));
                }
            }
            return commd;
        }

        public IDbCommand CreateCommandByObject<T>(string commandText, T parameter)
        {
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandText = ReplaceProfixTag(commandText);
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;

            var commandType = commd.GetType();
            var type = typeof(T);
            PropertyInfo[] props = type.GetProperties();

            foreach (var prop in props)
            {
                var handler = DynamicMethodCompiler.CreateGetHandler(type, prop);
                commd.Parameters.Add(CreateIDataParameter(TagName + GetColumnName(prop), handler(parameter), ParameterDirection.Input));
            }
            return commd;
        }
        public IDbCommand CreateInsertCommand<T>(T parameter)
        {
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;

            var type = typeof(T);

            string tableName = GetTableName(type);
            var props = type.GetProperties();
            var selectedTable = props.Where(a => _Tablename.Equals(a.Name)).FirstOrDefault();
            if (selectedTable != null)
            {
                var oVal = selectedTable.GetValue(parameter);
                if (oVal != null)
                {
                    tableName = oVal.ToString();
                }
            }

            var arr = GetParameterColumnNames(props);
            var sbSql = new StringBuilder();
            sbSql.Append("INSERT INTO ");
            sbSql.Append(tableName);
            sbSql.Append("(");
            sbSql.Append(arr[0]);
            sbSql.Append(")");

            sbSql.Append("VALUES(");
            sbSql.Append(arr[1]);
            sbSql.Append(")");

            commd.CommandText = sbSql.ToString();
            sbSql.Clear();
            foreach (var prop in props)
            {
                var val = DynamicMethodCompiler.CreateGetHandler(type, prop)(parameter);
                commd.Parameters.Add(CreateIDataParameter(TagName + GetColumnName(prop), val, ParameterDirection.Input));
            }
            return commd;

        }

        private static Dictionary<string, string> m_CachedTableName = new Dictionary<string, string>();
        private string GetTableName(Type type)
        {
            if (m_CachedTableName.ContainsKey(type.FullName))
                return m_CachedTableName[type.FullName];

            string name = type.Name;
            var tableAttr = type.GetCustomAttribute<TableAttribute>(true);
            if (tableAttr != null)
            {
                name = tableAttr.Name;
            }
            m_CachedTableName.Add(type.FullName, name);
            return name;
        }

        private string[] GetParameterColumnNames(PropertyInfo[] props, bool isTag = false)
        {
            var sbColumns = new StringBuilder();
            var sbTagColumns = new StringBuilder();
            foreach (var prop in props)
            {
                if (_Tablename.Equals(prop.Name))
                    continue;

                if (sbColumns.Length > 0)
                {
                    sbColumns.Append(",");
                }

                if (sbTagColumns.Length > 0)
                {
                    sbTagColumns.Append(",");
                }

                

                var name = GetColumnName(prop);
                sbColumns.Append(name);
                sbTagColumns.Append("@").Append(name);
            }
            return new[] { sbColumns.ToString(), sbTagColumns.ToString() };
        }

        private static ConcurrentDictionary<string, string> m_CachedPropertyInfo = new ConcurrentDictionary<string, string>();
        private string GetColumnName(PropertyInfo prop)
        {
            string key = prop.DeclaringType.FullName + prop.Name;
            if (m_CachedPropertyInfo.ContainsKey(key))
                return m_CachedPropertyInfo[key];

            string name = prop.Name;
            var attr = prop.GetCustomAttribute<ColumnAttribute>(true);
            if (attr != null)
            {
                name = attr.Name;
            }
            return m_CachedPropertyInfo.GetOrAdd(key, name);
        }
    }
}
