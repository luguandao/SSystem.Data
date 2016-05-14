using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace SSystem.Data
{
	public partial class Database
	{
		public IDataParameter CreateIDataParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input,int size=0)
		{
            DbParameter para = DbProviderFactory.CreateParameter();
            para.ParameterName = name;
            para.Direction = direction;
            para.Value = value;
            if (size > 0)
            {
                para.Size = size;
            }
            return para;

		}
	}
}
