using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.ConsoleTest.Models
{
    class AccNote
    {
        [Column(Name ="ssID")]
        public int Id { get; set; }
        public int Mrecno { get; set; }
        public string Code { get; set; }
        public string mNote { get; set; }
        public string PyCode { get; set; }
    }
}
