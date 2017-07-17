using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    /// <summary>
    /// 创建command对象时候的可选项
    /// </summary>
    public class CreateCommandOption
    {
        /// <summary>
        /// 获取或设置是否忽略主键
        /// </summary>
        public bool IgnorePrimaryKey { get; set; }
        /// <summary>
        /// 获取或设置忽略的列名
        /// </summary>
        public IList<string> IgnoreProperties { get; set; }
        /// <summary>
        /// 获取或设置指定的列名，优先以指定的列名为准
        /// </summary>
        public IList<string> OnlyProperties { get; set; }

        public IList<string> WhereProperties { get; set; }

        /// <summary>
        /// 指定要处理的属性，多属性值之间用逗号分隔
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public CreateCommandOption AddOnlyProperties(string properties)
        {
            if (OnlyProperties == null)
            {
                OnlyProperties = new List<string>();
            }
            return AddProperties(OnlyProperties, properties);
        }

        /// <summary>
        /// 指定要忽略的属性，多属性指之间用逗号分隔
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public CreateCommandOption AddIgnoreProperties(string properties)
        {
            if (IgnoreProperties == null)
            {
                IgnoreProperties = new List<string>();
            }
            return AddProperties(IgnoreProperties, properties);
        }

        public CreateCommandOption AddWhereProperties(string properties)
        {
            if (WhereProperties == null)
            {
                WhereProperties = new List<string>();
            }
            return AddProperties(WhereProperties, properties);
        }

        public bool HasDesignatedProperties(string Name)
        {
            if (OnlyProperties != null)
            {
                return OnlyProperties.Contains(Name);
            }

            if (IgnoreProperties != null)
            {
                return !IgnoreProperties.Contains(Name);
            }
            return false;
        }

        private CreateCommandOption AddProperties(IList<string> container, string properties)
        {
            if (!string.IsNullOrEmpty(properties))
            {
                var arr = properties.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 0)
                {
                    foreach (var item in arr)
                    {
                        container.Add(item);
                    }
                }
            }
            return this;
        }
    }
}
