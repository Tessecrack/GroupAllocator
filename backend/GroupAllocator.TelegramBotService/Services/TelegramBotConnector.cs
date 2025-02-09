using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using GroupAllocator.TelegramBotService.Core;


namespace GroupAllocator.TelegramBotService.Services
{
    public class TelegramBotConnector : BackgroundService
    {
        private readonly CancellationTokenSource _cancelationToken = new CancellationTokenSource();

        private readonly TelegramBotClient _telegramBotClient;

        private readonly TelegramMessagesProcessor _messagesProcessor;

        public Telegram.Bot.Types.User? Me { get; private set; }

        public TelegramBotConnector(TelegramUsersLocalStorage localStorage, TelegramBotClientOptions options)
        {
            _telegramBotClient = new TelegramBotClient(options, cancellationToken: _cancelationToken.Token);
            _messagesProcessor = new TelegramMessagesProcessor(_telegramBotClient, localStorage, _cancelationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Me = await _telegramBotClient.GetMe();

            if (Me == null)
            {
                throw new InvalidOperationException("БОТ НЕ ЗАПУЩЕН. ТРЕБУЕТСЯ ПЕРЕЗАПУСК.");
            }

            _telegramBotClient.OnMessage += _messagesProcessor.HandleMessage;
            _telegramBotClient.OnError += _messagesProcessor.HandleError;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelationToken.Cancel();

            _telegramBotClient.OnMessage -= _messagesProcessor.HandleMessage;
            _telegramBotClient.OnError -= _messagesProcessor.HandleError;

            await base.StopAsync(cancellationToken);
        }
    }
}