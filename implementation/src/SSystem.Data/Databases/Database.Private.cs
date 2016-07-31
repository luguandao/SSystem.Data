using SSystem.Data.Compiler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;

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

            string col = GetColumnName(propertyInfo);
            string typeName = typeof(T).FullName;

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
                    var type = typeof(T);
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        DynamicMethodCompiler.CreateSetHandler(type, propertyInfo)(target, Enum.ToObject(propertyInfo.PropertyType, value));
                        //propertyInfo.SetValue(target, Enum.ToObject(propertyInfo.PropertyType, value));
                    }
                    else
                    {
                        DynamicMethodCompiler.CreateSetHandler(type, propertyInfo)(target, value);
                        //propertyInfo.SetValue(target, value);
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
