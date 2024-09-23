using Dapper;
using InfinityNumerology.DataSource.Model;
using Npgsql;
using Telegram.Bot.Types;
namespace InfinityNumerology.DataSource
{
    public class DataBase : IDataBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public DataBase(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
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

            var sql = InsertBalanceSQL();
            var user_balance = new UserBalance()
            {
                balance_access = 3,
                user_Id = id
            };
            try
            {
                using(var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(sql,user_balance);
                }
            }
            catch (Exception exception)
            {
                throw;
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
                int currentBalance = await CheckUserBalance(id);

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

        public async Task<UserInfo> GetUserById(long id)
        {
            var sql = GetByIdSQL();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<UserInfo>(sql, new { user_Id = id });
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
        private string UpdateUserBalanceSQL()
        {
            var sql = @"UPDATE user_balance
                        SET balance_access = @newBalanceAccess
                        WHERE user_id = @user_Id";
            return sql;
        }
        private string InsertBalanceSQL()
        {
            var sql = $@"INSERT INTO user_balance (balance_access, user_id)
                        VALUES(@balance_access, @user_id)";
            return sql;
        }
        private string InsertSQL()
        {
            var sql = @"INSERT INTO user_info (user_id, firstname, username, bio, user_date)
                        VALUES (@User_Id, @FirstName, @UserName, @Bio, @user_Date)";
            return sql;
        }
        private string CheckSQL()
        {
            var sql = @"SELECT COUNT(1)
                        FROM user_info
                        WHERE user_id = @UserId";
            return sql;
        }
        private string GetByIdSQL()
        {
            var sql = @"SELECT user_id, firstname, username, bio, user_date
                        FROM user_info
                        WHERE user_id = @user_id";
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
