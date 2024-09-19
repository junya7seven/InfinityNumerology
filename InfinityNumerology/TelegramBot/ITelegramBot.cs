using Telegram.Bot.Types;
using Telegram.Bot;

namespace InfinityNumerology.TelegramBot
{
    public interface ITelegramBot
    {
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);

    }
}
