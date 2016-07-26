using DynamicCompilationSpike;
using SSystem.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data.ConsoleTest.Models;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Data.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DatabaseFactory.AddProviderFactory(System.Data.SqlClient.SqlClientFactory.Instance, DatabaseType.SqlServer);

            Dictionary<string, GetHandler> cached = new Dictionary<string, GetHandler>();
            //using (var db = DatabaseFactory.Create("ZDATA"))
            //{
            //    for (int i = 0; i < 1000000; i++)
            //    {
            //        db.CreateCommand("insert into ")
            //    }
            //}


            var item = new Item
            {
                Id = 1,
                Name = "abc",
                Age = 10
            };

            //Stopwatch sw = Stopwatch.StartNew();
            //var type = typeof(Item);
            //for (int i = 0; i < 100000; i++)
            //{
            //    var props = item.GetType().GetProperties();

            //    foreach (var p in props)
            //    {
            //        // p.GetValue(item);

            //        string key = type.FullName + p.Name;
            //        GetHandler handler;
            //        if (cached.ContainsKey(key))
            //        {
            //            handler = cached[key];
            //        }
            //        else
            //        {
            //            handler = DynamicMethodCompiler.CreateGetHandler(typeof(Item), p);
            //            cached.Add(key, handler);
            //        }

            //        var val = handler(item);
            //       // Console.WriteLine(val);
            //    }

            //}
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            using (var db = DatabaseFactory.Create("test"))
            {
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {

                    item = new Item
                    {
                        Id = i,
                        Name = "abc",
                        Age = 10
                    };

                    //var commd = db.CreateInsertCommand(item);
                    var commd = db.CreateCommand("insert into item,name,age) values(@name,@age)", new Hashtable {
                        { "id",item.Id},
                        { "name",item.Name},
                        { "age",item.Age}
                    });
                    // var commd = db.CreateCommandByObject("insert into item(id,name,age) values(@id,@name,@age)", item);
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);

            }


            //create insert sql 

        }
    }

    [Table(Name = "t_sms_sent_message")]
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        [Column(Name = "score_id")]
        public virtual int ScoreId { get; set; }
    }

    public class SubItem : Item
    {
        //  [Column(Name="score_id1")]
        public override int ScoreId
        {
            get
            {
                return base.ScoreId;
            }

            set
            {
                base.ScoreId = value;
            }
        }
    }


}
