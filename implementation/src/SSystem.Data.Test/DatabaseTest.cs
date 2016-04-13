using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SSystem.Data.Mappings;

namespace SSystem.Data.Test
{
    public class DatabaseTest
    {
        private const string _DbName = "ZDATA";
        [Fact]
        public void TestConnection()
        {
            using (var db = new Database(_DbName))
            {
                db.CurrentConnection.Open();

                Assert.Equal<DatabaseType>(DatabaseType.SqlServer, db.DatabaseType);
            }
        }

        [Fact]
        public void TestCreateCommand()
        {
            using (var db = new Database(_DbName))
            {
                string text = "select * from test";
                var commd = db.CreateCommand(text);
                Assert.Equal(text, commd.CommandText);
                Assert.Equal(db.CurrentConnection, commd.Connection);
            }
        }

        [Fact]
        public void TestCreateCommand2()
        {
            using (var db = new Database(_DbName))
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
            using (var db = new Database(_DbName))
            {
                var ds = db.Query("select * from AccNote", true, "mytable");
                Assert.Equal(18, ds.Tables[0].Rows.Count);
                Assert.Equal("mytable", ds.Tables[0].TableName);
            }
        }

        [Fact]
        public void TestQuery2()
        {
            using (var db = new Database(_DbName))
            {
                var ds = db.Query(db.CreateCommand("select * from AccNote"));
                Assert.Equal(18, ds.Tables[0].Rows.Count);
                Assert.Equal("table1", ds.Tables[0].TableName);
            }
        }

        [Fact]
        public void TestExecuteNonQuery()
        {
            using (var db = new Database(_DbName))
            {
                var n = db.ExecuteNonQuery("update AccNote set mNote='存入银行单证号码：' where ssID=93");
                Assert.Equal(1, n);
            }
        }

        [Fact]
        public void TestExecuteNonQuery2()
        {
            using (var db = new Database(_DbName))
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
            using (var db = new Database(_DbName))
            {
                using (var reader = db.CreateDataReader("select * from AccNote"))
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
            using (var db = new Database(_DbName))
            {
                var note = db.GetObjectList<AccNote>("select * from AccNote").First();
                Assert.True(note.Id > 0);
            }
        }
    }

    public class AccNote
    {
        [Column("ssId")]
        public int Id { get; set; }
        public int MrecNo { get; set; }
    }

}
