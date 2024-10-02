using Dapper;
using InfinityNumerology.DataSource.Model;
using Npgsql;
using System.Xml.Linq;
using Telegram.Bot.Types;
namespace InfinityNumerology.DataSource
{
    public class DataBase : IDataBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly DataBaseSet _dbset;
        public DataBase(IConfiguration configuration, DataBaseSet dataBase)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _dbset = dataBase;
        }
        public async Task InitializeAsync()
        {
            //await _dbset.CreateDataBase();
            await CreateTableIfNotExists();
        }

        public async Task<bool> CreateTableIfNotExists()
        {
            if (!await TableExists("user_info") ||
                !await TableExists("user_balance") ||
                !await TableExists("request_count"))
            {
                return await _dbset.CreateTable();
            }
            return true;
        }
        private async Task<bool> TableExists(string tableName)
        {
            var sql = $@"SELECT EXISTS (
                        SELECT 1 
                        FROM information_schema.tables 
                        WHERE table_name = '{tableName}'
                        );";
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QuerySingleAsync<bool>(sql);

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }
        public async Task<bool> OwnRequest(string SQL)
        {
            try
            {
                using(var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.ExecuteAsync(SQL) > 0;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"error - {exception.Message}");
                return false;
            }
        }
        public async Task UserInsert(Update update)
        {
            long userId = update.Message.Chat.Id;
            var sql = InsertSQL();
            try
            {
                if (!await CheckIdenticalUser(userId))
                {
                    var user = new UserInfo()
                    {
                        user_Id = update.Message.Chat.Id,
                        userName = update.Message.Chat.Username,
                        firstName = update.Message.Chat.FirstName,
                        bio = update.Message.Chat.Bio,
                        user_Date = DateTime.Now,
                    };
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var result = await connection.ExecuteAsync(sql, user);
                    }
                    await UserBalanceInsert(userId);
                    
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private async Task UserBalanceInsert(long id)
        {

            var checkSql = "SELECT COUNT(1) FROM user_balance WHERE user_id = @user_Id";
            var insertSql = "INSERT INTO user_balance (user_id, balance_access) VALUES(@user_Id, @balance_access)";
            var updateSql = "UPDATE user_balance SET balance_access = @balance_access WHERE user_id = @user_Id";

            var user_balance = new UserBalance()
            {
                balance_access = 20,
                user_Id = id
            };

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var exists = await connection.ExecuteScalarAsync<int>(checkSql, user_balance);

                    if (exists == 0)
                    {
                        await connection.ExecuteAsync(insertSql, user_balance);
                    }
                    else
                    {
                        await connection.ExecuteAsync(updateSql, user_balance);
                    }
                }
            }
            catch (Exception exception)
            {
                await RessetSequence();
                throw;
            }

        }
        private async Task RessetSequence()
        {
            var SQL = "SELECT setval('user_balance_user_balance_id_seq', (SELECT MAX(user_balance_id) FROM user_balance));";
            using(var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(SQL);
            }
        }
        public async Task<int> CheckUserBalance(long id)
        {
            try
            {
                var sql = CheckUserBalanceSQL();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QuerySingleOrDefaultAsync<int>(sql, new { user_Id = id });
                }
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public async Task<bool> UpdateUserBalance(long id, int newBalanceAccess)
        {
            try
            {
                //int currentBalance = await CheckUserBalance(id);

                var sql = UpdateUserBalanceSQL();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.ExecuteAsync(sql, new { NewBalanceAccess = newBalanceAccess, user_Id = id });
                    return result > 0; 
                }
            }
            catch (Exception exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> UpdateUserBalance(long id)
        {
            try
            {
                int newBalanceAccess = await CheckUserBalance(id)-1;

                var sql = UpdateUserBalanceStatSQL();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.ExecuteAsync(sql, new { NewBalanceAccess = newBalanceAccess, user_Id = id });
                    return result > 0;
                }
            }
            catch (Exception exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> CheckIdenticalUser(long id)
        {
            var sql = CheckSQL();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var count = await connection.QuerySingleAsync<int>(sql, new { user_Id = id });
                    return count > 0;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        public async Task<SuperUser> GetUserById(long id)
        {
            var sql = GetByIdSQL();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.QueryFirstOrDefaultAsync<SuperUser>(sql, new { user_Id = id });
                return result;
            }
        }

        public async Task RequestCount(long id, string commandName)
        {
            var sql = RequestCountSQL();
            try
            {
                using(var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(sql, new { UserId = id, CommandName = commandName });
                }
            }
            catch (Exception exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<UserInfo>> GetAllUsers()
        {
            var sql = GetAllSQL();
            using(var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.QueryAsync<UserInfo>(sql);
                if(!result.Any())
                {
                    return null;
                }
                return result;
            }
        }
        public async Task<List<long>> GetAllId()
        {
            var sql = GetAllIdSQL();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.QueryAsync<long>(sql);
                return result.ToList(); 
            }
        }
        private string UpdateUserBalanceStatSQL()
        {
            var sql = @"UPDATE user_balance
                        SET balance_access = @newBalanceAccess
                        WHERE user_id = @user_Id";
            return sql;
        }
        private string UpdateUserBalanceSQL()
        {
            var sql = @"UPDATE user_balance
                        SET balance_access = balance_access + @newBalanceAccess
                        WHERE user_id = @user_Id";
            return sql;
        }
        private string InsertBalanceSQL()
        {
            var sql = $@"INSERT INTO user_balance (user_id,balance_access)
                VALUES(@user_Id, @balance_access)
                ON CONFLICT (user_id) 
                DO UPDATE SET balance_access = EXCLUDED.balance_access";
            return sql;
        }
        private string InsertSQL()
        {
            var sql = @"INSERT INTO user_info (user_id, firstname, username, bio, user_date)
                        VALUES (@user_Id, @firstname, @username, @bio, @user_Date)";
            return sql;
        }
        private string CheckSQL()
        {
            var sql = @"SELECT COUNT(1)
                        FROM user_info
                        WHERE user_id = @user_Id";
            return sql;
        }
        private string GetByIdSQL()
        {
            var sql = @"SELECT u.user_id, u.firstname, u.username, u.bio, u.user_date, b.balance_access, r.last_request, r.count, r.command_name
                        FROM user_info u
                        LEFT JOIN user_balance b ON u.user_id = b.user_id
                        LEFT JOIN request_count r ON u.user_id = r.user_id
                        WHERE u.user_id = @user_id;";
            return sql;
        }
        private string GetAllSQL()
        {
            var sql = @"SELECT user_id, firstname, username, bio, user_date
                    FROM user_info";
            return sql;
        }
        private string CheckUserBalanceSQL()
        {
            var sql = @"SELECT balance_access
                        FROM user_balance
                        WHERE user_id = @user_id";
            return sql;
        }
        private string RequestCountSQL()
        {
            var sql = @"INSERT INTO request_count (user_id, last_request, count, command_name)
                        VALUES (@UserId, NOW(), 1, @CommandName)
                        ON CONFLICT (user_id)
                        DO UPDATE SET 
                            count = request_count.count + 1,
                            last_request = NOW(),
                            command_name = @CommandName;";
            return sql;
        }
        private string GetAllIdSQL()
        {
            var sql = @"SELECT user_id
                        FROM user_info";
            return sql;
        }
    }
}
