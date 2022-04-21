using System.Data;
using MySql.Data.MySqlClient;

namespace WriteForwardingTest
{
    internal class SchemaCreator
    {
        private readonly string _connectionString;

        private const string _creationSql = @"
CREATE USER IF NOT EXISTS 'test_user' IDENTIFIED BY 'data123!#';

CREATE DATABASE IF NOT EXISTS `test_db` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;

USE test_db;

CREATE TABLE IF NOT EXISTS `data` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `key_UNIQUE` (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

GRANT ALL PRIVILEGES ON test_db.* TO 'test_user'@'%';";

        public SchemaCreator(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void CreateSchema()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                using IDbCommand cmd = connection.CreateCommand();
                cmd.CommandText = _creationSql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create schema");
                Console.WriteLine(e);
            }
        }
    }
}
