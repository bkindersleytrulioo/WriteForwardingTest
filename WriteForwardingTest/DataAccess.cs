using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WriteForwardingTest
{
    internal class SqlRepository
    {
        public const string SetReadConsistency = "SET aurora_replica_read_consistency = @setting;";

        public static class Select
        {
            public const string Data = "SELECT * FROM data order by ID desc limit 10;";
            public const string LastID = " select LAST_INSERT_ID();";
        }

        public static class Insert
        {
            public const string Data = "INSERT INTO data (`Key`) VALUES (@Key);";
        }
    }

    internal class DataAccess
    {
        private readonly string _consistencyMode;
        private readonly string _connectionString;

        public DataAccess(string consistencySetting, string connectionString)
        {
            _consistencyMode = consistencySetting;
            _connectionString = connectionString;
        }

        private IDbConnection getOpenConnection()
        {
            var cxn = new MySqlConnection(_connectionString);
            cxn.Open();
            if (!string.IsNullOrEmpty(_consistencyMode))
            {
                try
                {
                    using var cmd = cxn.CreateCommand();
                    cmd.CommandText = SqlRepository.SetReadConsistency;
                    cmd.Parameters.Add(new MySqlParameter("@setting", _consistencyMode));
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Done setting aurora read consistency {_consistencyMode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set aurora read consistency {_consistencyMode}", ex);
                }
            }

            return cxn;
        }

        public IReadOnlyList<Data> GetData()
        {
            try
            {
                using var connection = getOpenConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = SqlRepository.Select.Data;
                using var res = cmd.ExecuteReader();
                var data = new List<Data>();
                while (res.Read())
                {
                    var id = res.GetInt32(0);
                    var key = res.GetString(1);
                    data.Add(new Data
                    {
                        ID = id,
                        Key = key
                    });
                }

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get data");
                Console.WriteLine(e);
                return new List<Data>();
            }
        }

        public int SaveDataAndGetIDInOneStatement(string key)
        {
            try
            {
                using var connection = getOpenConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = SqlRepository.Insert.Data + " " + SqlRepository.Select.LastID;
                cmd.Parameters.Add(new MySqlParameter("@Key", key));
                using var res = cmd.ExecuteReader();
                if (res.Read())
                {
                    var id = res.GetInt32(0);
                    Console.WriteLine($"Returned id is {id}");
                    return id;
                }
                else
                {
                    Console.WriteLine("res.Read is false");
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed inserting");
                Console.WriteLine(e);
                return 0;
            }
        }

        public int SaveDataAndGetIDInTwoStatements(string key)
        {
            try
            {
                using var connection = getOpenConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = SqlRepository.Insert.Data;
                cmd.Parameters.Add(new MySqlParameter("@Key", key));
                cmd.ExecuteNonQuery();
                Console.WriteLine("Insert Executed");

                cmd.CommandText = SqlRepository.Select.LastID;
                using var res = cmd.ExecuteReader();
                if (res.Read())
                {
                    var id = res.GetInt32(0);
                    Console.WriteLine($"Returned id is {id}");
                    return id;
                }
                else
                {
                    Console.WriteLine("res.Read is false");
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed inserting");
                Console.WriteLine(e);
                return 0;
            }
        }
    }
}
