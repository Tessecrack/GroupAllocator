using GroupAllocator.Models.TelegramModels;

namespace GroupAllocator.TelegramBotService.Interfaces
{
    public interface ITelegramBot
    {
        Task SendMessage(TelegramMessageModel message);
    }
}
