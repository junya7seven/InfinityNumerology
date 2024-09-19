using InfinityNumerology.DataSource;
using InfinityNumerology.OpenAI;
using InfinityNumerology.Service.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Telegram.Bot.Types;

namespace InfinityNumerology.Service
{
    public class ServiceResponse
    {
        private readonly OpenAIService _openAI;
        private readonly ILogger<ServiceResponse> _logger;
        IConfiguration _configuration;
        private readonly IDataBase _db;
        public ServiceResponse(OpenAIService openAI, ILogger<ServiceResponse> logger, IConfiguration configuration, IDataBase db)
        {
            _openAI = openAI;
            _logger = logger;
            _configuration = configuration;
            _db = db;
        }
        public async Task<string> DistributorAsync(DateTime date, string command)
        {
            string? prompt = null;
            string systemHelp, assistantHelp;
            switch(command)
            {
                case "Расшифровка даты рождения":
                    prompt = TextHepler.DecodingBirthdayPrompt(date,out systemHelp, out assistantHelp);
                    break;
                case "Совместимость знаков зодиака":
                    prompt = TextHepler.ZodiakPrompt(date, out systemHelp, out assistantHelp);
                    break;
                case "Матрица судьбы":
                    prompt = TextHepler.MartixOfFate(date, out systemHelp, out assistantHelp);
                    break;
                default:
                    return "Неизвестная ошибка, попробуйте ещё раз";
            }
            try
            {
                if(prompt != null)
                {
                var resultMessage = await Message(date, prompt, systemHelp, assistantHelp);
                return resultMessage;
                }
                return "Ошибка обработки промпта";
            }
            catch (Exception exception)
            {
                return $"Ошибка запроса: {exception.Message}";
            }


        }
        
        public async Task<string> Message(DateTime date, string prompt, string systemHelp, string assistantHelp)
        {
            var result = await _openAI.MessageResponse(prompt, systemHelp, assistantHelp);
            return result;
        }

        public async Task<string> DistributorWithoutDateAsync(string command)
        {
            switch(command)
            {
                case "Как производится расшифрока?":
                    return TextHepler.DecodingInfo();
                case "Обратная связь":
                    return TextHepler.FeedBack();
                case "Информация":
                    return TextHepler.Information();
                default:
                    return "Неизвестная ошибка, попробуйте ещё раз";
            }
        }

        public async Task<bool> InsertIntoTable(Update update)
        {
            try
            {
                await _db.UserInsert(update);
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            
            
        }

        public async Task<string> GetUsersForAdmin(string command)
        {
            if(command == "/getall")
            {
                StringBuilder usersString = new StringBuilder();
                var users = await _db.GetAllUsers();
                foreach (var user in users)
                {
                    usersString.Append($"{user.user_Id} | {user.userName} | {user.firstName} | {user.bio} | {user.user_Date}\n");
                }
                return usersString.ToString();
            }
            else if(command.StartsWith("/getbyid="))
            {
                if(long.TryParse(command.Substring(9), out var id))
                {
                    StringBuilder userString = new StringBuilder();
                    var user = await _db.GetUserById(id);
                    userString.Append(user.user_Id + "|");
                    userString.Append(user.userName + "|");
                    userString.Append(user.firstName + "|");
                    userString.Append(user.bio + "|");
                    userString.Append(user.user_Date + "|");
                    return userString.ToString();
                }
            }
            return "User not found";
            
        }

    }
}
