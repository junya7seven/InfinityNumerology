using Telegram.Bot.Types;

namespace InfinityNumerology.DataSource.Model
{
    public class UserBalance
    {
        public int balance_id {  get; set; }
        public int balance_access { get; set; } = 3;
        public long user_Id { get; set; }
    }
}
