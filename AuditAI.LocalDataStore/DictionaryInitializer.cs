﻿﻿using System.Data.SQLite;
using Newtonsoft.Json.Linq;

namespace Auditai.LocalDataStore
{
    public static class DictionaryInitializer
    {
        /// <summary>
        /// 确保数据字典表中有初始数据
        /// 如果不存在，创建一个空的字典结构
        /// </summary>
        public static void EnsureDictionaryData(string connectionString)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string[] dicTypes = {
                "TableCollect", "CellCollect", "LedgerValidate"
            };

            foreach (var dicType in dicTypes)
            {
                bool exists = (long)new SQLiteCommand(
                    $"SELECT COUNT(*) FROM DataDictionary WHERE DicType = '{dicType}'",
                    conn).ExecuteScalar() == 0;

                if (exists)
                {
                    var emptyData = new JObject
                    {
                        ["version"] = 0,
                        [dicType == "TableCollect" ? "Tables" :
                         dicType == "CellCollect" ? "Cells" : "Ledgers"] = new JObject()
                    };

                    new SQLiteCommand(@"
                        INSERT INTO DataDictionary (DicType, Version, Data)
                        VALUES (@Type, 0, @Data)", conn)
                    {
                        Parameters = {
                            new("@Type", dicType),
                            new("@Data", emptyData.ToString())
                        }
                    }.ExecuteNonQuery();
                }
            }
        }
    }
}
