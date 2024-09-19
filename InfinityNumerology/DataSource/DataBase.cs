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
                }
            }
            catch (Exception exception)
            {
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
                    var count = await connection.QuerySingleAsync<int>(sql, new { UserId = id });
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
    }
}
