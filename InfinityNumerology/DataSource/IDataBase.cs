using InfinityNumerology.DataSource.Model;
using Telegram.Bot.Types;

namespace InfinityNumerology.DataSource
{
    public interface IDataBase
    {
        Task UserInsert(Update update);
        Task<bool> CheckIdenticalUser(long id);
        Task<UserInfo> GetUserById(long id);
        Task<IEnumerable<UserInfo>> GetAllUsers();
    }
}
