using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public partial class Database
    {
        private void AssignValue<T>(IDataReader reader, IEnumerable<string> allFieldNames, PropertyInfo propertyInfo, T target)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            string col = propertyInfo.Name;
            string typeName = typeof(T).FullName;
            string key = typeName + col;
            var cachedValue = MemoryCache.Default.Get(key);
            if (cachedValue!=null)
            {
                col = cachedValue.ToString();
            }
            else
            {
                var attr = propertyInfo.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                if (attr != null && !string.IsNullOrEmpty(attr.Name))
                {
                    col = attr.Name;
                }
                MemoryCache.Default.Add(key, col, DateTime.Now.AddMinutes(TimeoutOfCaching));
            }

            if (allFieldNames.Where(a => a.Equals(col, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                var index = reader.GetOrdinal(col);
                var value = reader.GetValue(index);
                if (value == DBNull.Value)
                {
                    value = null;
                }

                if (index > -1)
                {
                    //propertyInfo.PropertyType.BaseType.Name
                    if (propertyInfo.PropertyType.IsEnum)
                    {

                        propertyInfo.SetValue(target, Enum.ToObject(propertyInfo.PropertyType, value));
                    }
                    else
                    {
                        propertyInfo.SetValue(target, value);
                    }
                }
            }

        }

        private IDbConnection CreateConnection(string connectionString)
        {
            if (DbProviderFactory == null)
            {
                DbProviderFactory = CreateDbProviderFactory();
            }
            IDbConnection icon = DbProviderFactory.CreateConnection();
            icon.ConnectionString = connectionString;
            return icon;
        }

        private DbProviderFactory CreateDbProviderFactory() => DbProviderFactories.GetFactory(m_ProviderName);

        private string ReplaceProfixTag(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;
            sql = sql.Replace("@", TagName);
            return sql;
        }

        private IEnumerable<string> GetNames(IDataReader reader)
        {
            var names = new List<string>();
            int count = reader.FieldCount;
            for (int i = 0; i < count; i++)
            {
                names.Add(reader.GetName(i));
            }
            return names;
        }
    }
}
