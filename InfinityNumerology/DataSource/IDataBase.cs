using InfinityNumerology.DataSource.Model;
using Telegram.Bot.Types;

namespace InfinityNumerology.DataSource
{
    public interface IDataBase
    {
        Task UserInsert(Update update);
        Task<bool> CheckIdenticalUser(long id);
        Task<SuperUser> GetUserById(long id);
        Task<IEnumerable<UserInfo>> GetAllUsers();
        Task<int> CheckUserBalance(long id);
        Task<bool> UpdateUserBalance(long id, int newBalanceAccess);
        Task<bool> UpdateUserBalance(long id);
        Task RequestCount(long id, string commandName);
        Task<List<long>> GetAllId();
        Task InitializeAsync();
    }
}
