using Bartz24.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using Dapper;
using System.Collections.Generic;

namespace Bartz24.FF13_2_LR
{
    public class DB3Database
    {
        public static Dictionary<int, T> GetEntries<T>(string path, string tableName) where T : DataStoreDB3Entry
        {
            try
            {
                using (IDbConnection con = CreateConnection(path))
                {
                    return con.Query<T>($"select * from \"{tableName}\"").ToDictionary(t => t.main_id, t => t);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read database file at: " + path, e);
            }
        }

        public static void Save<T>(string path, string tableName, Dictionary<int, T> data) where T : DataStoreDB3Entry
        {
            try
            {
                using (IDbConnection con = CreateConnection(path))
                {
                    int maxId = con.Query<int>($"select max(main_id) from \"{tableName}\"").First();
                    List<T> insert = data.Where(p => p.Key > maxId).Select(p => p.Value).ToList();
                    List<T> update = data.Where(p => p.Key <= maxId).Select(p => p.Value).ToList();
                    con.Execute(GetUpdateQuery<T>(tableName), update);
                    if (insert.Count > 0)
                        con.Execute(GetInsertQuery<T>(tableName), insert);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to save database file at: " + path, e);
            }
        }

        private static string GetUpdateQuery<T>(string tableName) where T : DataStoreDB3Entry
        {
            SqlBuilder builder = new SqlBuilder()
                .Where("main_id = @main_id");
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ForEach(p =>
            {
                builder = builder.Set($"{p.Name} = @{p.Name}");
            });
            return builder.AddTemplate($"update \"{tableName}\" /**set**/ /**where**/").RawSql;
        }

        private static string GetInsertQuery<T>(string tableName) where T : DataStoreDB3Entry
        {
            List<string> names = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();
            return $"insert into \"{tableName}\"({string.Join(",", names)}) values ({string.Join(",", names.Select(s => "@" + s))})";
        }

        public static int GetStringArraySize(string path)
        {
            try
            {
                using (IDbConnection con = CreateConnection(path))
                {
                    return con.Query<int>($"select sub_entry_size_in_bits from \"{"06 !!strArrayInfo table"}\"").First();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read string array size in file at: " + path, e);
            }
        }


        private static SQLiteConnection CreateConnection(string path)
        {
            return new SQLiteConnection($"Data Source=\"{path}\";Version=3;");
        }
    }
}
