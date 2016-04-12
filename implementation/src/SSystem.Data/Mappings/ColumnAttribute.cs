using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data.Mappings
{
    public class ColumnAttribute:Attribute
    {
        public string ColumnName { get; }
        public ColumnAttribute(string name)
        {
            ColumnName = name;
        }
    }
}
