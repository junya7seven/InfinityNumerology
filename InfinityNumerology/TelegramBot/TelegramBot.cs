using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using InfinityNumerology.DataSource;
using InfinityNumerology.Service;
using InfinityNumerology.Service.AdminCommands;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace InfinityNumerology.TelegramBot
{
    public class TelegramBot : ITelegramBot
    {
        private readonly Dictionary<long, bool> _awaitingDateInput = new Dictionary<long, bool>();
        private readonly Dictionary<long, string> _lastPressedButton = new Dictionary<long, string>();
        private readonly ServiceResponse _serviceResponse;
        private readonly Admin _admin;
        private readonly IConfiguration _configuration;
        private readonly string adminIdString;
        private readonly long adminId;
        public TelegramBot(ServiceResponse serviceResponse, Admin admin, IConfiguration configuration)
        {
            _serviceResponse = serviceResponse;
            _admin = admin;
            _configuration = configuration;
            adminIdString = _configuration.GetSection("BotConfiguration:adminID").Value;
            adminId = long.Parse(adminIdString);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                if(chatId == adminId && messageText.StartsWith("/") && messageText != "/start")
                {
                    await HadleAdminCommand(botClient, update, cancellationToken, chatId, messageText);
                }
                else if (messageText == "/start")
                {
                    Console.WriteLine($"{update.Message.Chat.Id} - {update.Message.Chat.Username} - {update.Message.Chat.FirstName}");
                    await _serviceResponse.InsertIntoTable(update);
                    await HandleStartCommand(botClient, chatId, cancellationToken);
                }
                else if (IsButtonClick(messageText))
                {
                    await HandleButtonClick(botClient, chatId, messageText, cancellationToken);
                }
                else if (_awaitingDateInput.ContainsKey(chatId) && _awaitingDateInput[chatId])
                {
                    await HandleDateInput(botClient, chatId, messageText, cancellationToken);
                }
                else
                {
                    var text = "Неизвестная команда. Пожалуйста, выберите действие..";
                    await ServiceResponse.SendMessage(botClient, chatId, cancellationToken,text, adminId);
                }
            }
        }

        private async Task HadleAdminCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, long chatId, string messageText)
        {
            if (messageText.StartsWith("/notification"))
            {
                string[] command = messageText.Split('=');
                var message = command[1];
                if(string.IsNullOrEmpty(message) || message.Length < 1)
                {
                    return;
                }
                var users = await _admin.NotificationUsers();
                /*List<long> users = new List<long>();
                users.Add(5860197616);
                users.Add(5860197616);
                users.Add(5860197616);
                users.Add(12121);*/
                foreach (var id in users)
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: id,
                            text: message,
                            cancellationToken: cancellationToken);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        if (ex.ErrorCode == 403)
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: adminId,
                            text: $"Пользователь с id {id} заблокировал бота.",
                            cancellationToken: cancellationToken);
                        }
                        else if (ex.ErrorCode == 404)
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: adminId,
                            text: $"Пользователь с id {id} не найден.",
                            cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: adminId,
                            text: $"Ошибка при отправке сообщения пользователю {id}: {ex.Message}",
                            cancellationToken: cancellationToken);
                        }
                    }
                }
            }
            else
            {
                var result = await _admin.CheckCommand(messageText,botClient,cancellationToken,adminId);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: result,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task HandleStartCommand(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
{
                    new KeyboardButton[] { "\xD83D\xDD0DРасшифровка даты рождения", "\x264BСовместимость знаков зодиака" },
                    new KeyboardButton[] { "\xD83D\xDD2EМатрица судьбы" },
                    new KeyboardButton[] { "\xD83D\xDCC3Как производится расшифрока?", "\xD83D\xDC69\u200D\xD83D\xDCBCОбратная связь" },
                    new KeyboardButton[] { "\x2139Информация" },
                    new KeyboardButton[] { "\x2696Узнать баланс", "\xD83D\xDCB5Пополнить баланс" }
                })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false, 
                InputFieldPlaceholder = "Выберите действие" 
            };

            var text = "Выберите действие:";
            await ServiceResponse.SendMessage(botClient, chatId, cancellationToken, text, adminId, replyKeyboard);
        }

        private async Task HandleButtonClick(ITelegramBotClient botClient, long chatId, string buttonText, CancellationToken cancellationToken)
        {
            string pattern = "[\xD83D\xDD0D\x264B\xD83D\xDD2E\xD83D\xDCC3\xD83D\xDC69\u200D\xD83D\xDCBC\x2139\x2696\xD83D\xDCB5]";
            buttonText = Regex.Replace(buttonText, pattern, "");

            if (buttonText.Contains("Как производится расшифрока?") || buttonText.Contains("Обратная связь") || buttonText.Contains("Информация"))
            {
                var text = await _serviceResponse.DistributorWithoutDateAsync(buttonText);
                await ServiceResponse.SendMessage(botClient, chatId, cancellationToken, text, adminId);
            }
            else if (buttonText.Contains("Узнать баланс") || buttonText.Contains("Пополнить баланс"))
            {
                await _serviceResponse.BalanceRequest(botClient, cancellationToken, buttonText, chatId, adminId);
            }
            else
            {
                _lastPressedButton[chatId] = buttonText;
                _awaitingDateInput[chatId] = true;
                var text = "Введите дату рождения (в формате ДД/ММ/ГГГГ):";
                await ServiceResponse.SendMessage(botClient, chatId, cancellationToken, text, adminId);
            }
        }

        private async Task HandleDateInput(ITelegramBotClient botClient, long chatId, string dateInput, CancellationToken cancellationToken)
        {
            if (DateTime.TryParseExact(dateInput, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime parsedDate))
            {
                var text = "Ожидайте обработки запроса.....";
                await ServiceResponse.SendMessage(botClient, chatId, cancellationToken, text, adminId);
                string pressedButton = _lastPressedButton.ContainsKey(chatId) ? _lastPressedButton[chatId] : "Неизвестно";
                var response = await _serviceResponse.DistributorAsync(parsedDate, pressedButton, chatId);
                await ServiceResponse.SendMessage(botClient, chatId, cancellationToken, response, adminId);

                _awaitingDateInput[chatId] = false;
            }
            else
            {
                var text = "Некорректная дата. Пожалуйста, введите дату в формате ДД/ММ/ГГГГ.";
                await ServiceResponse.SendMessage(botClient, chatId, cancellationToken, text, adminId);
            }
        }

        private bool IsButtonClick(string messageText)
        {
            string pattern = "[\xD83D\xDD0D\x264B\xD83D\xDD2E\xD83D\xDCC3\xD83D\xDC69\u200D\xD83D\xDCBC\x2139\x2696\xD83D\xDCB5]";
            messageText = Regex.Replace(messageText, pattern, "");
            return messageText.Contains("Расшифровка даты рождения") ||
                   messageText.Contains("Совместимость знаков зодиака") ||
                   messageText.Contains("Матрица судьбы") ||
                   messageText.Contains("Как производится расшифрока?") ||
                   messageText.Contains("Информация") ||
                   messageText.Contains("Обратная связь") ||
                   messageText.Contains("Узнать баланс") ||
                   messageText.Contains("Пополнить баланс");
        }
    }
}
