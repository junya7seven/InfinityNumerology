using InfinityNumerology.DataSource;
using InfinityNumerology.DataSource.Model;
using InfinityNumerology.OpenAI;
using InfinityNumerology.Service.AdminCommands;
using InfinityNumerology.Service.Text;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;

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
        public async Task<string> DistributorAsync(DateTime date, string command, long id)
        {
            string? prompt = null;
            string systemHelp, assistantHelp;
            if(!await CheckUserBalance(id))
            {
                return "Не достаточно средств на балансе. Пополните баланс, либо обратитесь \"Обратная связь\"";
            }
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
                    var resultMessage = await Message(date, prompt, systemHelp, assistantHelp, id);
                    await _db.RequestCount(id, command);
                    Console.WriteLine($"user-{id} send a request {command}");
                    return resultMessage;
                }
                return "Ошибка обработки промпта";
            }
            catch (Exception exception)
            {
                return $"Ошибка запроса: {exception.Message}";
            }
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

        public async Task BalanceRequest(ITelegramBotClient botClient, CancellationToken cancellationToken,string command, long chatId,long adminId)
        {
            if(command == "Узнать баланс")
            {
                int userBalance = await _db.CheckUserBalance(chatId);
                var text = $"Ваш баланс - {userBalance}";
                await SendMessage(botClient,chatId,cancellationToken,text,adminId);

            }
            else if(command == "Пополнить баланс")
            {
                var text = $"Скоро появится, временно кол-во запросов увеличено на 20. Если у вас закончились запросы, обратитель в \"Обратная связь\"";
                await SendMessage(botClient, chatId, cancellationToken, text, adminId);
            }
            else
            {
                var text = $"Неизвестная ошибка";
                await SendMessage(botClient, chatId, cancellationToken, text, adminId);
            }
        }

        private async Task<bool> CheckUserBalance(long id)
        {
            int userBalance = await _db.CheckUserBalance(id);
            if(userBalance > 0)
            {
                return true;
            }    
            return false;
        }

        public async Task<string> Message(DateTime date, string prompt, string systemHelp, string assistantHelp, long id)
        {
            var result = await _openAI.MessageResponse(prompt, systemHelp, assistantHelp);
            if (result == null)
            {
                return "Неизвестная ошибка, попробуйте ещё раз";
            }
            if (!await _db.UpdateUserBalance(id))
            {
                return "Не удлось узнать баланс";
            }
            return result;
        }

        public async Task<bool> InsertIntoTable(Update update)
        {
            try
            {
                await _db.UserInsert(update);
                Console.WriteLine($"New user {update.Message.Chat.Id}");
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public static async Task SendMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, string message, long adminId, IReplyMarkup? replyMarkup = null)
        {
            try
            {
                await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: message,
                            replyMarkup: replyMarkup,
                            cancellationToken: cancellationToken);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException exception)
            {
                if (exception.ErrorCode == 403)
                {
                    await botClient.SendTextMessageAsync(
                    chatId: adminId,
                    text: $"Пользователь с id {chatId} заблокировал бота.",
                    cancellationToken: cancellationToken);
                }
                else if (exception.ErrorCode == 404)
                {
                    await botClient.SendTextMessageAsync(
                    chatId: adminId,
                    text: $"Пользователь с id {chatId} не найден.",
                    cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                    chatId: adminId,
                    text: $"Ошибка при отправке сообщения пользователю {chatId}: {exception.Message}",
                    cancellationToken: cancellationToken);
                }
            }
        }
    }
}
