using SSystem.Data;
using System;
using System.Collections.Generic;
using System.Data.ConsoleTest.Models;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var db = new Database("ZDATA"))
            {
                System.Diagnostics.Stopwatch sw = new Stopwatch();
                sw.Start();
                var id = db.ExecuteScalar<int>(db.CreateCommand("select ssID from AccNote"));
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            }
        }
    }
}
