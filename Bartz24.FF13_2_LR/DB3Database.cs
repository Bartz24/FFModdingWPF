using Bartz24.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using System.Collections.Generic;
using Z.Dapper.Plus;

namespace Bartz24.FF13_2_LR
{
    public class DB3Database
    {
        public static Dictionary<int, T> GetEntries<T>(string path, string tableName) where T : DataStoreDB3Entry
        {
            using (IDbConnection con = CreateConnection(path))
            {
                return con.Query<T>($"select * from \"{tableName}\"").ToDictionary(t => t.main_id, t => t);
            }
        }

        public static void Save<T>(string path, string tableName, Dictionary<int, T> data) where T : DataStoreDB3Entry
        {
            DapperPlusManager.Entity<T>().Table(tableName).Key(t => t.main_id);
            using (IDbConnection con = CreateConnection(path))
            {
                int maxId = con.Query<int>($"select max(main_id) from \"{tableName}\"").First();
                List<T> insert = data.Where(p => p.Key > maxId).Select(p => p.Value).ToList();
                List<T> update = data.Where(p => p.Key <= maxId).Select(p => p.Value).ToList();
                con.BulkUpdate(update);
                if (insert.Count > 0)
                    con.BulkInsert(insert);
            }
        }

        public static int GetStringArraySize(string path)
        {
            using (IDbConnection con = CreateConnection(path))
            {
                return con.Query<int>($"select sub_entry_size_in_bits from \"{"06 !!strArrayInfo table"}\"").First();
            }
        }


        private static SQLiteConnection CreateConnection(string path)
        {
            return new SQLiteConnection($"Data Source=\"{path}\";Version=3;");
        }
    }
}
