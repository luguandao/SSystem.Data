﻿using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SSystem.Data.Test
{
    public class DatabaseTest
    {
        private const string _DbName = "ZDATA";

        static DatabaseTest()
        {
            DatabaseFactory.AddProviderFactory(System.Data.SqlClient.SqlClientFactory.Instance, DatabaseType.SqlServer);
        }
        [Fact]
        public void TestConnection()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                db.Connection.Open();

                Assert.Equal<DatabaseType>(DatabaseType.SqlServer, db.DatabaseType);
            }
        }

        [Fact]
        public void TestCreateCommand()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                string text = "select * from test";
                var commd = db.CreateCommand(text);
                Assert.Equal(text, commd.CommandText);
                Assert.Equal(db.Connection, commd.Connection);
            }
        }

        [Fact]
        public void TestCreateCommand2()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                string text = "select * from AccNote where ssID=@id";
                var commd = db.CreateCommand(text, new Dictionary<string, int>() {
                    { "id",91}
                });
                Assert.Equal(1, commd.Parameters.Count);
                var ds = db.Query(commd);
                Assert.Equal(1, ds.Tables[0].Rows.Count);
            }
        }

        [Fact]
        public void TestQuery()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var ds = db.Query("select * from AccNote", true, "mytable");
                Assert.Equal(18, ds.Tables[0].Rows.Count);
                Assert.Equal("mytable", ds.Tables[0].TableName);
            }
        }

        [Fact]
        public void TestQuery2()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var ds = db.Query(db.CreateCommand("select * from AccNote"));
                Assert.Equal(18, ds.Tables[0].Rows.Count);
                Assert.Equal("table1", ds.Tables[0].TableName);
            }
        }

        [Fact]
        public void TestExecuteNonQuery()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var n = db.ExecuteNonQuery("update AccNote set mNote='存入银行单证号码：' where ssID=93");
                Assert.Equal(1, n);
            }
        }

        [Fact]
        public void TestExecuteNonQuery2()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var ds = db.Query("select * from AccNote");
                var dt = ds.Tables[0];
                dt.TableName = "AccNote";
                dt.Rows[0]["mNote"] = "存入银行单证号码2：";
                var n = db.ExecuteNonQuery(dt);
                Assert.Equal(1, n);
            }
        }

        [Fact]
        public void TestCreateDataReader()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                using (var reader = db.ExecuteReader("select * from AccNote"))
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("ssID");
                        Assert.False(id == 0);
                    }
                }
            }
        }

        [Fact]
        public void TestGetObject()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var note = db.QueryObject<AccNote>("select * from AccNote").First();
                Assert.True(note.Id > 0);
            }
        }

        [Fact]
        public void TestExecuteScalar()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var id = db.ExecuteScalar<int>("select ssId from AccNote");
                Assert.True(id > 0);
            }
        }

        [Fact]
        public void TestExecuteScalarObject()
        {
            System.Diagnostics.Debug.WriteLine("start testing");
            System.Diagnostics.Debug.Flush();
            using (var db = DatabaseFactory.Create(_DbName))
            {
                object id = db.ExecuteScalar(db.CreateCommand("select ssId from AccNote"));
                Assert.True(Convert.ToInt32(id)>0);
            }
        }

        [Fact]
        public void TestGetFirstColumn()
        {
            using (var db = DatabaseFactory.Create(_DbName))
            {
                var list = db.GetFirstColumn("select * from AccNote");
                Assert.True(list.Count > 0);
            }
        }
    }

    public class AccNote
    {
        [Column(Name ="ssID")]
        public int Id { get; set; }
        public int MrecNo { get; set; }
    }

}
