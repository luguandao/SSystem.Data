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

        /// <summary>
        /// 生成insert语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public IDbCommand CreateInsertCommand<T>(T parameter, CreateCommandOption option = null)
        {
            if (option == null)
            {
                option = new CreateCommandOption();
            }
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;

            var type = typeof(T);
            var props = SplitInsertPropertiesByOption(option, GetColumnProperties(type));

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
            props = SelectProps(props, values);
            var columns = GetParameterColumnNames(props, values, option.IgnorePrimaryKey);
            var sbSql = new StringBuilder();
            sbSql.Append("INSERT INTO ");
            sbSql.Append(tableName);
            sbSql.Append("(");
            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0)
                {
                    sbSql.Append(",");
                }
                sbSql.Append(columns[i]);
            }
            sbSql.Append(")");

            sbSql.Append(" VALUES(");
            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0)
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

        private IEnumerable<PropertyInfo> SplitUpdateOrDeletePropertiesByOption(CreateCommandOption option, IEnumerable<PropertyInfo> props)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();

            if (option.WhereProperties != null && option.WhereProperties.Any())
            {
                list.AddRange(props.Where(a => option.WhereProperties.Contains(a.Name)));
            }
            else
            {
                var pri = props.FirstOrDefault(a => GetColumnAttribute(a).IsPrimaryKey);
                if (pri != null)
                {
                    list.Add(pri);
                }
            }

            if (option.OnlyProperties != null && option.OnlyProperties.Any())
            {
                list.AddRange(props.Where(a => option.OnlyProperties.Contains(a.Name)));
            }
            else
            {
                foreach (var item in props)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }
            if (option.IgnoreProperties != null && option.IgnoreProperties.Any())
            {
                foreach (var item in option.IgnoreProperties)
                {
                    var selected = list.FirstOrDefault(a => a.Name == item);
                    if (selected != null)
                    {
                        list.Remove(selected);
                    }
                }
            }
            return list.Distinct();
        }

        private IEnumerable<PropertyInfo> SplitInsertPropertiesByOption(CreateCommandOption option, IEnumerable<PropertyInfo> props)
        {
            if (option.OnlyProperties != null && option.OnlyProperties.Any())
            {
                props = props.Where(a => option.OnlyProperties.Contains(a.Name));
            }
            else if (option.IgnoreProperties != null && option.IgnoreProperties.Any())
            {
                props = props.Where(a => !option.IgnoreProperties.Contains(a.Name));
            }

            return props;
        }

        /// <summary>
        /// 生成update语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public IDbCommand CreateUpdateCommand<T>(T parameter, CreateCommandOption option = null)
        {
            if (option == null)
            {
                option = new CreateCommandOption();
            }
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;

            var type = typeof(T);
            var props = SplitUpdateOrDeletePropertiesByOption(option, GetColumnProperties(type));

            var values = CalculteValues(parameter, props);
            string tableName = GetTableName(type);
            props = SelectProps(props, values);
            var columns = GetParameterColumnNamesWithoutPrimaryKey(props, values, option);
            var sbSql = new StringBuilder();

            sbSql.Append("UPDATE ");
            sbSql.Append(tableName);
            sbSql.Append(" SET ");
            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0)
                {
                    sbSql.Append(",");
                }
                sbSql.Append(columns[i]);
                sbSql.Append("=");
                sbSql.Append($"@{columns[i]}");
            }

            sbSql.Append(" WHERE ");

            if (option.WhereProperties == null || !option.WhereProperties.Any())
            {
                string primaryKeyName = GetPrimaryKeyName(props);
                sbSql.Append(primaryKeyName);
                sbSql.Append("=");
                sbSql.Append("@");
                sbSql.Append(primaryKeyName);
            }
            else
            {
                var whereProps = SplitWherePropertiesByOption(option, GetColumnProperties(type)).ToArray();
                var whereValues = CalculteValues(parameter, whereProps);
                whereProps = SelectProps(whereProps, whereValues);
                for (var i = 0; i < whereProps.Length; i++)
                {
                    if (i > 0)
                    {
                        sbSql.Append(" AND ");
                    }
                    sbSql.AppendFormat("{0}=@{0}", GetColumnName(whereProps[i]));
                }
            }

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

        /// <summary>
        /// 生成Delete sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public IDbCommand CreateDeleteCommand<T>(T parameter, CreateCommandOption option = null)
        {
            if (option == null)
            {
                option = new CreateCommandOption();
            }
            var commd = Connection.CreateCommand();
            commd.Transaction = Transaction;
            commd.CommandTimeout = DefaultCommandTimeoutBySeconds;

            var type = typeof(T);
            var props = GetColumnProperties(type);
            var values = CalculteValues(parameter, props);
            string tableName = GetTableName(type);
            props = SelectProps(props, values);

            var primaryKeyName = GetPrimaryKeyName(props);

            StringBuilder sbSql = new StringBuilder();
            sbSql.Append($"DELETE FROM {tableName} WHERE ");
            if (option.WhereProperties == null || !option.WhereProperties.Any())
            {
                sbSql.Append($"{primaryKeyName}=@{primaryKeyName}");
                PropertyInfo primaryInfo = props.First(a => GetColumnName(a) == primaryKeyName);
                commd.Parameters.Add(CreateIDataParameter(TagName + primaryKeyName, values[primaryInfo.Name], ParameterDirection.Input));
            }
            else
            {
                var whereProps = SplitWherePropertiesByOption(option, GetColumnProperties(type)).ToArray();
                var whereValues = CalculteValues(parameter, whereProps);
                whereProps = SelectProps(whereProps, whereValues);
                for (var i = 0; i < whereProps.Length; i++)
                {
                    if (i > 0)
                    {
                        sbSql.Append(" AND ");
                    }
                    sbSql.AppendFormat("{0}=@{0}", GetColumnName(whereProps[i]));
                }
                foreach (var prop in whereProps)
                {
                    var val = values[prop.Name];
                    if (val == null)
                    {
                        val = DBNull.Value;
                    }
                    commd.Parameters.Add(CreateIDataParameter(TagName + GetColumnName(prop), val, ParameterDirection.Input));
                }
            }
            commd.CommandText = sbSql.ToString();
            return commd;
        }

        private IEnumerable<PropertyInfo> SplitWherePropertiesByOption(CreateCommandOption option, IEnumerable<PropertyInfo> props)
        {
            if (option.WhereProperties != null && option.WhereProperties.Any())
            {
                props = props.Where(a => option.WhereProperties.Contains(a.Name)).ToArray();
            }
            return props;
        }

        private Dictionary<string, object> CalculteValues<T>(T parameter, IEnumerable<PropertyInfo> props)
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

        private string[] GetParameterColumnNames(IEnumerable<PropertyInfo> props, Dictionary<string, object> values, bool ignorePrimaryKey)
        {
            var columns = new List<string>();
            foreach (var prop in props)
            {
                if (_Tablename.Equals(prop.Name))
                    continue;
                var name = GetColumnName(prop);
                if (string.IsNullOrEmpty(name))
                    continue;
                var val = values[prop.Name];
                if (val == null)
                    continue;
                var isDefaultValue = false;
                switch (val.GetType().Name.ToLower())
                {
                    case "int64":
                        isDefaultValue = Convert.ToInt64(val) == default(long);
                        break;
                    case "int32":
                        isDefaultValue = Convert.ToInt32(val) == default(int);
                        break;
                    case "int16":
                        isDefaultValue = Convert.ToInt16(val) == default(short);
                        break;
                    case "datetime":
                        isDefaultValue = Convert.ToDateTime(val) == default(DateTime);
                        break;
                }
                var attr = GetColumnAttribute(prop);
                if (attr != null && attr.IsDbGenerated && isDefaultValue)
                    continue;
                if (attr != null && attr.IsPrimaryKey && ignorePrimaryKey)
                    continue;


                columns.Add(name);
            }
            return columns.ToArray();
        }

        private string[] GetParameterColumnNamesWithoutPrimaryKey(IEnumerable<PropertyInfo> props, Dictionary<string, object> values, CreateCommandOption option)
        {
            var columns = new List<string>();
            foreach (var prop in props)
            {
                if (_Tablename.Equals(prop.Name))
                    continue;
                var name = GetColumnName(prop);
                if (string.IsNullOrEmpty(name))
                    continue;
                var attr = GetColumnAttribute(prop);
                if (attr != null && (attr.IsDbGenerated || attr.IsPrimaryKey))
                    continue;
                var val = values[prop.Name];
                if (val == null)
                    continue;
                if (option != null && option.WhereProperties != null && option.WhereProperties.Any() && option.WhereProperties.Contains(prop.Name))
                    continue;

                columns.Add(name);
            }
            return columns.ToArray();
        }

        private string GetPrimaryKeyName(IEnumerable<PropertyInfo> props)
        {
            var selected = props.FirstOrDefault(a => GetColumnAttribute(a).IsPrimaryKey);
            if (selected == null)
                return string.Empty;
            return GetColumnName(selected);
        }

        private PropertyInfo[] SelectProps(IEnumerable<PropertyInfo> props, Dictionary<string, object> values)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (var prop in props)
            {
                if (_Tablename.Equals(prop.Name))
                    continue;
                var name = GetColumnName(prop);
                if (string.IsNullOrEmpty(name))
                    continue;

                var isDefaultValue = false;
                var val = values[prop.Name];
                if (val == null)
                    continue;

                switch (val.GetType().Name.ToLower())
                {
                    case "int64":
                        isDefaultValue = Convert.ToInt64(val) == default(long);
                        break;
                    case "int32":
                        isDefaultValue = Convert.ToInt32(val) == default(int);
                        break;
                    case "int16":
                        isDefaultValue = Convert.ToInt16(val) == default(short);
                        break;
                    case "datetime":
                        isDefaultValue = Convert.ToDateTime(val) == default(DateTime);
                        break;
                }


                var attr = _CachedPropertyInfoColumnAttributes[prop.PropertyType.FullName + "." + prop.Name];
                if (attr != null && attr.IsDbGenerated && isDefaultValue)
                    continue;

                list.Add(prop);
            }
            return list.ToArray();
        }

        private static ConcurrentDictionary<string, string> _CachedPropertyInfo = new ConcurrentDictionary<string, string>();

        private string GetColumnName(PropertyInfo prop)
        {
            string key = $"GetColumnName.{prop.PropertyType.FullName}.{prop.Name}"; 
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
            string key = prop.PropertyType.FullName + "." + prop.Name;
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

        private IEnumerable<PropertyInfo> GetColumnProperties(Type type)
        {
            var props = type.GetProperties();
            var customType = typeof(ColumnAttribute);
            return props.Where(a => a.CustomAttributes.Any(b => b.AttributeType == customType));
        }
    }
}
