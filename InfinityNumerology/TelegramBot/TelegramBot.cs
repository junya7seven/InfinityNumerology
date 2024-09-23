using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using InfinityNumerology.DataSource;
using InfinityNumerology.Service;
using InfinityNumerology.Service.AdminCommands;

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
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Неизвестная команда. Пожалуйста, выберите действие.",
                        cancellationToken: cancellationToken
                    );
                }
            }
        }

        private async Task HadleAdminCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, long chatId, string messageText)
        {
            if (messageText.StartsWith("/notification"))
            {
                string[] command = messageText.Split('=');
                var message = command[1];
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
                var result = await _admin.CheckCommand(messageText);
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
            new KeyboardButton[] { "Расшифровка даты рождения", "Совместимость знаков зодиака" },
            new KeyboardButton[] { "Матрица судьбы"},
            new KeyboardButton[] { "Как производится расшифрока?", "Обратная связь" },
            new KeyboardButton[] { "Информация"}
        })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите действие:",
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private async Task HandleButtonClick(ITelegramBotClient botClient, long chatId, string buttonText, CancellationToken cancellationToken)
        {
            if (buttonText == "Как производится расшифрока?" || buttonText == "Обратная связь" || buttonText == "Информация")
            {
                var text = await _serviceResponse.DistributorWithoutDateAsync(buttonText);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                _lastPressedButton[chatId] = buttonText;
                _awaitingDateInput[chatId] = true;

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введите дату рождения (в формате ДД/ММ/ГГГГ):",
                    cancellationToken: cancellationToken
                );
            }
        }

        private async Task HandleDateInput(ITelegramBotClient botClient, long chatId, string dateInput, CancellationToken cancellationToken)
        {
            if (DateTime.TryParseExact(dateInput, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime parsedDate))
            {
                string pressedButton = _lastPressedButton.ContainsKey(chatId) ? _lastPressedButton[chatId] : "Неизвестно";
                var response = await _serviceResponse.DistributorAsync(parsedDate, pressedButton, chatId);

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response,
                    cancellationToken: cancellationToken
                );

                _awaitingDateInput[chatId] = false;
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Некорректная дата. Пожалуйста, введите дату в формате ДД/ММ/ГГГГ.",
                    cancellationToken: cancellationToken
                );
            }
        }

        private bool IsButtonClick(string messageText)
        {
            return messageText == "Расшифровка даты рождения" ||
                   messageText == "Совместимость знаков зодиака" ||
                   messageText == "Матрица судьбы" ||
                   messageText == "Остальное" ||
                   messageText == "Как производится расшифрока?" ||
                   messageText == "Информация" ||
                   messageText == "Обратная связь";
        }
    }
}
