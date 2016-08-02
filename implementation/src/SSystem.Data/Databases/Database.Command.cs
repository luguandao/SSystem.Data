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
        public IDbCommand CreateCommand(string commandText = null) => CreateCommandByDictionary(commandText, null);

        /// <summary>
        /// 生成Command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IDbCommand CreateCommandByDictionary(string commandText, IDictionary parameters)
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

        private IDbCommand CreateCommandByObject<T>(string commandText, T parameter)
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

        public IDbCommand CreateCommand<T>(string commandText, T parameter)
        {
            var dic = parameter as IDictionary;
            if (dic != null)
                return CreateCommandByDictionary(commandText, dic);
            return CreateCommandByObject<T>(commandText, parameter);
        }

        public IDbCommand CreateInsertCommand<T>(T parameter)
        {
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;

            var type = typeof(T);
            var props = type.GetProperties();
            var values = CalculteValues(parameter, props);
            string tableName = GetTableName(type);

            var selectedTable = props.Where(a => _Tablename.Equals(a.Name)).FirstOrDefault();
            if (selectedTable != null)
            {
                var oVal = values[selectedTable.Name];
                if (oVal != null)
                {
                    tableName = oVal.ToString();
                }
            }

            var columns = GetParameterColumnNames(props, values);
            var sbSql = new StringBuilder();
            sbSql.Append("INSERT INTO ");
            sbSql.Append(tableName);
            sbSql.Append("(");
            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0 && i == columns.Length - 1)
                {
                    sbSql.Append(",");
                }
                sbSql.Append(columns[i]);
            }
            sbSql.Append(")");

            sbSql.Append("VALUES(");
            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0 && i == columns.Length - 1)
                {
                    sbSql.Append(",");
                }
                sbSql.Append(TagName);
                sbSql.Append(columns[i]);
            }
            sbSql.Append(")");

            commd.CommandText = sbSql.ToString();
            sbSql.Clear();
            foreach (var prop in props)
            {
                var val = values[prop.Name];
                if (val == null)
                {
                    val = DBNull.Value;
                }
                commd.Parameters.Add(CreateIDataParameter(TagName + GetColumnName(prop), val, ParameterDirection.Input));
            }
            return commd;
        }

        private Dictionary<string, object> CalculteValues<T>(T parameter, PropertyInfo[] props)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            var type = typeof(T);
            foreach (var prop in props)
            {
                values.Add(prop.Name, DynamicMethodCompiler.CreateGetHandler(type, prop)(parameter));
            }
            return values;
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

        private string[] GetParameterColumnNames(PropertyInfo[] props, Dictionary<string, object> values)
        {
            var columns = new List<string>();
            foreach (var prop in props)
            {
                if (_Tablename.Equals(prop.Name))
                    continue;
                var name = GetColumnName(prop);
                if (string.IsNullOrEmpty(name))
                    continue;
                var attr = _CachedPropertyInfoColumnAttributes[prop.DeclaringType.FullName + "." + prop.Name];
                if (attr != null && attr.IsDbGenerated)
                    continue;
                var val = values[prop.Name];
                if (val == null)
                    continue;
                columns.Add(name);
            }
            return columns.ToArray();
        }

        private static ConcurrentDictionary<string, string> _CachedPropertyInfo = new ConcurrentDictionary<string, string>();
        
        private string GetColumnName(PropertyInfo prop)
        {
            string key = prop.DeclaringType.FullName + "." + prop.Name;
            if (_CachedPropertyInfo.ContainsKey(key))
                return _CachedPropertyInfo[key];

            string name = prop.Name;

            ColumnAttribute attr = GetColumnAttribute(prop);
            if (attr != null && !string.IsNullOrEmpty(attr.Name))
            {
                name = attr.Name;
            }

            _CachedPropertyInfo.TryAdd(key, name);
            return name;
        }

        private static ConcurrentDictionary<string, ColumnAttribute> _CachedPropertyInfoColumnAttributes = new ConcurrentDictionary<string, ColumnAttribute>();
        private ColumnAttribute GetColumnAttribute(PropertyInfo prop)
        {
            ColumnAttribute attr;
            string key = prop.DeclaringType.FullName + "." + prop.Name;
            if (_CachedPropertyInfoColumnAttributes.ContainsKey(key))
            {
                attr = _CachedPropertyInfoColumnAttributes[key];
            }
            else
            {
                attr = prop.GetCustomAttribute<ColumnAttribute>(true);
                _CachedPropertyInfoColumnAttributes.TryAdd(key, attr);
            }
            return attr;
        }
    }
}
