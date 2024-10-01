using InfinityNumerology.DataSource;
using System.Text;
using Telegram.Bot;

namespace InfinityNumerology.Service.AdminCommands
{
    public class Admin 
    {
        private readonly IDataBase _db;
        private readonly ServiceResponse _service;
        public Admin(IDataBase db, ServiceResponse service)
        {
            _db = db;
            _service = service;
        }
        private string SplitCommand(string command,out long id, out int balance)
        {
            string[] result = command.Split('=');
            id = 0;
            balance = 0;
            if(result.Length >= 2 )
            {
                id = long.Parse(result[1]);
            }
            if(result.Length == 3 )
            {
                balance = int.Parse(result[2]);
            }
            return result[0];
        }
        public async Task<string> CheckCommand(string command, ITelegramBotClient botClient, CancellationToken cancellationToken, long adminId)
        {
            command = SplitCommand(command, out long id, out int balance);

            try
            {
                switch (command)
                {

                    case "/getall":
                        StringBuilder usersString = new StringBuilder();
                        var users = await _db.GetAllUsers();
                        if(users == null)
                        {
                            return "Пользователей нет";
                        }
                        foreach (var item in users)
                        {
                            usersString.Append($"{item.user_Id} | {item.userName} | {item.firstName} | {item.bio} | {item.user_Date}\n");
                        }
                        return usersString.ToString();

                    case "/getbyid":
                        var user = await _db.GetUserById(id);
                        var userinfo = $"username - {user.username}, firstname - {user.firstname}, bio - {user.bio}, registerDate - {user.user_date}\n,userBalance - {user.balance_access}, commandName - {user.command_name}, countRequest - {user.count}, lastRequest - {user.last_request}";
                        return userinfo;

                    case "/upbalance":
                        var newUserBalance = await _db.UpdateUserBalance(id, balance);
                        var text = "";
                        if(!newUserBalance)
                        {
                            text = $"Не удалось изменить баланс";

                        }
                        text = $"Ваш баланс был обновлен, доступно запросов - {await _db.CheckUserBalance(id)}";

                        await ServiceResponse.SendMessage(botClient,id,cancellationToken,text,adminId);
                        return $"New balance for user {id} - {balance}";

                    case "/checkbalance":
                        var userBalance = await _db.CheckUserBalance(id);
                        return $"User balance {id} - {userBalance}";
                    case "/checkrequest":

                        return "qw";
                    case "/commands":
                        return @"/getall, /getbyid=5860197616, /upbalance=5860197616=5, 
                                 /checkbalance=5860197616, /notification=(message)
                                 /checkrequest=5860197616";

                    default:
                        return "error";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }
        }
        public async Task<List<long>> NotificationUsers()
        {
            var ids = await _db.GetAllId();
            return ids;
        }
    }
}
