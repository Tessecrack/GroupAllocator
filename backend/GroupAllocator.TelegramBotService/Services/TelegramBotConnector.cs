using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using GroupAllocator.TelegramBotService.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace GroupAllocator.TelegramBotService.Services
{
    public class TelegramBotConnector : BackgroundService
    {
        private readonly CancellationTokenSource _cancelationToken = new CancellationTokenSource();

        private TelegramBotClient _telegramBotClient;

        private TelegramMessagesProcessor _messagesProcessor;

        private readonly TelegramUsersLocalStorage _localStorage;

        private readonly TelegramBotClientOptions _botOptions;

        public Telegram.Bot.Types.User? Me { get; private set; }

        public TelegramBotConnector(TelegramUsersLocalStorage localStorage, TelegramBotClientOptions options)
        {
            _localStorage = localStorage;
            _botOptions = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_botOptions.Token))
            {
                return;
            }

            _telegramBotClient = new TelegramBotClient(_botOptions, cancellationToken: _cancelationToken.Token);
            _messagesProcessor = new TelegramMessagesProcessor(_telegramBotClient, _localStorage, _cancelationToken);

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
            if (Me == null)
            {
                return;
            }

            _cancelationToken.Cancel();

            _telegramBotClient.OnMessage -= _messagesProcessor.HandleMessage;
            _telegramBotClient.OnError -= _messagesProcessor.HandleError;

            await base.StopAsync(cancellationToken);
        }
    }
}