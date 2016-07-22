using SSystem.Data;
using System.Data.ConsoleTest.Models;
using System.Diagnostics;

namespace System.Data.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DatabaseFactory.AddProviderFactory(System.Data.SqlClient.SqlClientFactory.Instance, DatabaseType.SqlServer);
            using (var db = DatabaseFactory.Create("ZDATA"))
            {
                var obj = db.GetObjectList<AccNote>(db.CreateCommand("select * from AccNote"));
                foreach (var item in obj)
                {
                    Console.WriteLine(item.Id);
                }
            }
        }
    }
}
