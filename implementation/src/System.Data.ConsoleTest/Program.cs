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

           
            using (var db = DatabaseFactory.Create("test"))
            {

                var commd = db.CreateCommand("select * from Item where id=@id", new { id=1});

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
