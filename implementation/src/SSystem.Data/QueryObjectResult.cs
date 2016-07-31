using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public class QueryObjectResult<T>
    {
        public IEnumerable<T> Objects { get; set; }
        public int Count { get; set; }
    }
}
