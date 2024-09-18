using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Globalization;
using InfinityNumerology.Service;
using InfinityNumerology.OpenAI;

var builder = WebApplication.CreateBuilder(args);

var botClient = new TelegramBotClient("Your_Token");
builder.Services.AddSingleton(botClient);
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<Service>();


var app = builder.Build();

app.MapGet("/", () => "Hello, I'm a Telegram bot!");

Dictionary<long, bool> awaitingDateInput = new Dictionary<long, bool>();

Dictionary<long, string> lastPressedButton = new Dictionary<long, string>();

var cts = new CancellationTokenSource();
botClient.StartReceiving(
    new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
    cancellationToken: cts.Token
);

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    var service = app.Services.GetRequiredService<Service>();
    if (update.Type == UpdateType.Message && update.Message!.Text != null)
    {
        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;
        var info = $"{update.Message.Contact} {update.Message.Location} {update.Message.Chat}";
        await Console.Out.WriteLineAsync(info);
        if (messageText == "/start")
        {
            await HandleStartCommand(botClient, chatId, cancellationToken);
        }
        else if (IsButtonClick(messageText))
        {
            await HandleButtonClick(botClient, chatId, messageText,service, cancellationToken);
        }
        else if (awaitingDateInput.ContainsKey(chatId) && awaitingDateInput[chatId])
        {
            await HandleDateInput(botClient, chatId, messageText,service, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "����������� �������. ����������, �������� ��������.",
                cancellationToken: cancellationToken
            );
        }
    }
}

async Task HandleStartCommand(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
{
    var replyKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[] { "����������� ���� ��������", "������������� ������ �������" },
        new KeyboardButton[] { "������� ������"},
        new KeyboardButton[] { "��� ������������ ����������?", "�������� �����" },
        new KeyboardButton[] { "����������"}
    })
    {
        ResizeKeyboard = true
    };

    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "�������� ��������:",
        replyMarkup: replyKeyboard,
        cancellationToken: cancellationToken
    );
}

async Task HandleButtonClick(ITelegramBotClient botClient, long chatId, string buttonText, Service service, CancellationToken cancellationToken)
{

    if (buttonText == "��� ������������ ����������?" || buttonText == "�������� �����" || buttonText == "����������")
    {
        var text = await service.DistributorWithoutDateAsync(buttonText);
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            cancellationToken: cancellationToken
        );
    }
    else
    {
        // ��������� ��������� ������
        lastPressedButton[chatId] = buttonText;

        // ������������� ���������, ��� ��� ������� ����
        awaitingDateInput[chatId] = true;

        // ����������� � ������������ ����
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "������� ���� �������� (� ������� ��/��/����):",
            cancellationToken: cancellationToken
        ) ;
    }
}

async Task HandleDateInput(ITelegramBotClient botClient, long chatId, string dateInput, Service service, CancellationToken cancellationToken)
{
    if (DateTime.TryParseExact(dateInput, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime parsedDate))
    {
        string pressedButton = lastPressedButton.ContainsKey(chatId) ? lastPressedButton[chatId] : "����������";
        string response = YourMethod(parsedDate, pressedButton);
        
        var test = await service.DistributorAsync(parsedDate, pressedButton);
            
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: test,
            cancellationToken: cancellationToken
        );

        awaitingDateInput[chatId] = false;
    }
    else
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "������������ ����. ����������, ������� ���� � ������� ��/��/����.",
            cancellationToken: cancellationToken
        );
    }
}

bool IsButtonClick(string messageText)
{
    return messageText == "����������� ���� ��������" ||
           messageText == "������������� ������ �������" ||
           messageText == "������� ������" ||
           messageText == "���������" ||
           messageText == "��� ������������ ����������?" ||
           messageText == "����������" ||
           messageText == "�������� �����";
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"��������� ������: {exception.Message}");
    return Task.CompletedTask;
}

app.Run();

string YourMethod(DateTime date, string buttonPressed)
{
    return $"��������� ��� ������ '{buttonPressed}' � ���� {date.ToString("dd/MM/yyyy")}";
}
