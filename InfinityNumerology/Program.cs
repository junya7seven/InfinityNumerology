using InfinityNumerology.DataSource;
using InfinityNumerology.OpenAI;
using Telegram.Bot.Polling;
using Telegram.Bot;
using InfinityNumerology.Service;
using InfinityNumerology.TelegramBot;
using InfinityNumerology.Service.AdminCommands;

var builder = WebApplication.CreateBuilder(args);

var botClient = new TelegramBotClient("your_token");
builder.Services.AddSingleton(botClient);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IDataBase, DataBase>();
builder.Services.AddSingleton<ITelegramBot, TelegramBot>();
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<ServiceResponse>();
builder.Services.AddSingleton<Admin>();

var app = builder.Build();

app.MapGet("/", () => "Hello, I'm a Telegram bot!");

var botService = app.Services.GetRequiredService<ITelegramBot>();
var cts = new CancellationTokenSource();
botClient.StartReceiving(
    new DefaultUpdateHandler(botService.HandleUpdateAsync, HandleErrorAsync),
    cancellationToken: cts.Token
);

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"Произошла ошибка: {exception.Message}");
    return Task.CompletedTask;
}

app.Run();
