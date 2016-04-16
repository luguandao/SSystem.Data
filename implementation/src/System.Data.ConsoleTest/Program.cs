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
            //            using (var context = new Database("test"))
            //            {
            ////                Console.WriteLine(@"create DATABASE test01
            ////on primary
            ////(
            ////name='test01_data',
            ////filename='C:\temp\test01_data.mdf',
            ////size=5mb,
            ////maxsize=100mb,
            ////filegrowth=15%
            ////)
            ////log on
            ////(
            ////name='test01_log',
            ////filename='c:\temp\test01_log.ldf',
            ////size=2mb,
            ////filegrowth=1mb
            ////)
            ////");
            ////                context.ExecuteNonQuery(@"create DATABASE test01
            ////on primary
            ////(
            ////name='test01_data',
            ////filename='C:\temp\test01_data.mdf',
            ////size=5mb,
            ////maxsize=100mb,
            ////filegrowth=15%
            ////)
            ////log on
            ////(
            ////name='test01_log',
            ////filename='c:\temp\test01_log.ldf',
            ////size=2mb,
            ////filegrowth=1mb
            ////)
            ////");


            //            }


            //        }

            var creater = new SqlServerDatabaseCreator("test01", AppDomain.CurrentDomain.BaseDirectory);
            creater.Create("test");
        }
    }
}
