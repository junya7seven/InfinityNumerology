using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InfinityNumerology.DataSource
{
    public class DataBaseSet
    {
        private readonly string _connectionString;

        public DataBaseSet(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<bool> CreateTable()
        {
            var createUserInfoTable = @"
        CREATE TABLE IF NOT EXISTS user_info (
            user_id BIGINT PRIMARY KEY,
            firstname VARCHAR(128),
            username VARCHAR(128),
            bio TEXT,
            user_date TIMESTAMP NOT NULL
        );";

            var createUserBalanceTable = @"
        CREATE TABLE IF NOT EXISTS user_balance (
            user_balance_id SERIAL PRIMARY KEY,
            user_id BIGINT UNIQUE,
            balance_access INTEGER,
            FOREIGN KEY (user_id) REFERENCES user_info(user_id) ON DELETE CASCADE
        );";

            var createRequestCountTable = @"
        CREATE TABLE IF NOT EXISTS request_count (
            request_count_id SERIAL PRIMARY KEY,
            user_id BIGINT UNIQUE,
            last_request TIMESTAMP NOT NULL,
            count INT,
            command_name TEXT,
            FOREIGN KEY (user_id) REFERENCES user_info(user_id) ON DELETE CASCADE
        );";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    await connection.ExecuteAsync(createUserInfoTable);

                    await connection.ExecuteAsync(createUserBalanceTable);

                    await connection.ExecuteAsync(createRequestCountTable);

                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return false;
            }
        }


    }
}
