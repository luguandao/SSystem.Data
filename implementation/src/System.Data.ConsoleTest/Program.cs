using SSystem.Data;
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
                System.Diagnostics.Stopwatch sw = new Stopwatch();
                sw.Start();
                var id = db.ExecuteScalar<int>(db.CreateCommand("select ssID from AccNote"));
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            }
        }
    }
}
