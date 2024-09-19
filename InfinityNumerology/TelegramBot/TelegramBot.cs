using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using InfinityNumerology.DataSource;
using InfinityNumerology.Service;

namespace InfinityNumerology.TelegramBot
{
    public class TelegramBot : ITelegramBot
    {
        private readonly Dictionary<long, bool> _awaitingDateInput = new Dictionary<long, bool>();
        private readonly Dictionary<long, string> _lastPressedButton = new Dictionary<long, string>();
        private readonly ServiceResponse _serviceResponse;

        public TelegramBot(ServiceResponse serviceResponse)
        {
            _serviceResponse = serviceResponse;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                if(chatId == 5860197616)
                {
                    var test = await _serviceResponse.GetUsersForAdmin(messageText);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: test,
                        cancellationToken: cancellationToken);
                }

                if (messageText == "/start")
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
                var response = await _serviceResponse.DistributorAsync(parsedDate, pressedButton);

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
