using InfinityNumerology.DataSource;
using System.Text;

namespace InfinityNumerology.Service.AdminCommands
{
    public class Admin 
    {
        private readonly IDataBase _db;
        public Admin(IDataBase db)
        {
            _db = db;
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
        public async Task<string> CheckCommand(string command)
        {

            command = SplitCommand(command, out long id, out int balance);
            
            switch (command)
            {

                case "/getall":
                    StringBuilder usersString = new StringBuilder();
                    var users = await _db.GetAllUsers();
                    foreach (var item in users)
                    {
                        usersString.Append($"{item.user_Id} | {item.userName} | {item.firstName} | {item.bio} | {item.user_Date}\n");
                    }
                    return usersString.ToString();

                case "/getbyid":
                    StringBuilder userString = new StringBuilder();
                    var user = await _db.GetUserById(id);
                    userString.Append(user.user_Id + "|");
                    userString.Append(user.userName + "|");
                    userString.Append(user.firstName + "|");
                    userString.Append(user.bio + "|");
                    userString.Append(user.user_Date + "|");
                    return userString.ToString();

                case "/upbalance":
                    var newUserBalance = await _db.UpdateUserBalance(id, balance);
                    return $"New balance for user {id} - {balance}";

                case "/checkbalance":
                    var userBalance = await _db.CheckUserBalance(id);
                    return $"User balance {id} - {userBalance}";
                    
                default:
                    return "error";
            }
        }
        public async Task<List<long>> NotificationUsers()
        {
            var ids = await _db.GetAllId();
            return ids;
        }
    }
}
