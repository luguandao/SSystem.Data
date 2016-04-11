using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public enum DbCommandType
    {
        /// <summary>
        /// 不表示任何的类型
        /// </summary>
        NonCommand = 0,
        /// <summary>
        /// 查询语句
        /// </summary>
        SelectCommand = 1,
        /// <summary>
        /// 插入语句
        /// </summary>
        InsertCommand = 2,
        /// <summary>
        /// 更新语句
        /// </summary>
        UpdateCommand = 4,
        /// <summary>
        /// 删除语句
        /// </summary>
        DeleteCommand = 8,
        /// <summary>
        /// 同时支持select,insert,update,delete
        /// </summary>
        AllCommand = 15
    }
}
