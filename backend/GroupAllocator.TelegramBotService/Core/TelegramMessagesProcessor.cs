using GroupAllocator.Models.TelegramModels;
using GroupAllocator.TelegramBotService.Constants;
using GroupAllocator.TelegramBotService.Models;
using GroupAllocator.TelegramBotService.Services;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupAllocator.TelegramBotService.Core
{
    public class TelegramMessagesProcessor
    {
        private TelegramUsersLocalStorage _usersLocalStorage;

        private readonly TelegramBotClient _telegramBotClient;

        private readonly CancellationTokenSource _cancellationToken;

        public TelegramMessagesProcessor(TelegramBotClient client, 
            TelegramUsersLocalStorage localStorage, CancellationTokenSource cts)
        {
            _usersLocalStorage = localStorage;
            _telegramBotClient = client;
            _cancellationToken = cts;
        }

        internal async Task HandleMessage(Message message, UpdateType updateType)
        {
            if (message is null || string.IsNullOrWhiteSpace(message.Text)
                || updateType != UpdateType.Message || message.From == null)
            {
                return;
            }

            await HandleMessage(message);
        }

        internal async Task HandleError(Exception exception, HandleErrorSource errorSource)
        {

        }

        private async Task HandleMessage(Telegram.Bot.Types.Message message)
        {
            try
            {
                var fromId = message.From!.Id;

                if (_usersLocalStorage.IdsOwners.TryGetValue(fromId, out OwnerModel? owner))
                {
                    await DialogOwner(message, owner);
                }
                else if (_usersLocalStorage.IdsParticipants.TryGetValue(fromId, out ParticipantModel? part))
                {
                    await DialogParticipant(message, part);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task DialogOwner(Telegram.Bot.Types.Message message, OwnerModel owner)
        {
            if (owner == null || owner.Me == null || owner.Me.TelegramUser == null)
            {
                return;
            }

            switch (message.Text)
            {
                case TelegramCommand.START:
                    await SendStartMessageOwner(owner);
                    break;
                case TelegramCommand.MY_ROLE:
                    await SendMessage(
                        new TelegramMessageModel()
                        {
                            FromId = owner.Me.TelegramUser.ChatId,
                            ToId = owner.Me.TelegramUser.ChatId,
                            Content = "Ваша роль - Руководитель"
                        });
                    break;
                case TelegramCommand.TUTORIAL:
                    await SendTutorialMessageOwner(owner);
                    break;

                case TelegramCommand.INFO_GROUP:
                    await SendInfoAboutGroup(owner.Me);
                    break;

                case TelegramCommand.LIST_PARTICIPANTS:
                    await SendListParticipants(owner);
                    break;

                default:
                    await SendMessageToParticipant(owner, message.Text!);
                    break;
            }
        }

        private async Task DialogParticipant(Telegram.Bot.Types.Message message, ParticipantModel part)
        {
            if (part == null || part.Me == null || part.Me.TelegramUser == null
                || part.Owner == null || part.Owner.TelegramUser == null)
            {
                return;
            }

            switch (message.Text)
            {
                case TelegramCommand.START:
                    await SendStartMessageParticipant(part);
                    break;
                case TelegramCommand.MY_ROLE:
                    await SendMessage(
                        new TelegramMessageModel()
                        {
                            FromId = part.Me.TelegramUser.ChatId,
                            ToId = part.Me.TelegramUser.ChatId,
                            Content = "Ваша роль - Участник"
                        });
                    break;
                case TelegramCommand.TUTORIAL:
                    await SendTutorialMessageParticipant(part);
                    break;
                case TelegramCommand.INFO_GROUP:
                    await SendInfoAboutGroup(part.Me);
                    break;

                default:
                    var telegramMessage = new TelegramMessageModel()
                    {
                        FromId = part.Me.TelegramUser.ChatId,
                        ToId = part.Owner.TelegramUser.ChatId,
                        Content = message.Text!
                    };
                    await SendMessage(telegramMessage);
                    break;
            }
        }
        private async Task SendStartMessageOwner(OwnerModel owner)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[] { new KeyboardButton("Моя роль"), new KeyboardButton("Руководство") },
                    new KeyboardButton[] { new KeyboardButton("Информация о группе") },
                    new KeyboardButton[] { new KeyboardButton("Список участников") }
                })
            {
                ResizeKeyboard = true,
            };

            await _telegramBotClient.SendMessage(
                owner.Me!.TelegramUser!.ChatId,
                "Выберите пункт меню",
                replyMarkup: replyKeyboard);
        }

        private async Task SendStartMessageParticipant(ParticipantModel participant)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[] { new KeyboardButton("Моя роль"), new KeyboardButton("Руководство") },
                    new KeyboardButton[] { new KeyboardButton("Информация о группе") },
                })
            {
                ResizeKeyboard = true,
            };

            await _telegramBotClient.SendMessage(
                participant.Me!.TelegramUser!.ChatId,
                "Выберите пункт меню",
                replyMarkup: replyKeyboard);
        }

        private async Task SendTutorialMessageOwner(OwnerModel owner)
        {
            string text = "Для того, чтобы отправить сообщение участнику группы " +
                "Вы должны указать в начале сообщения порядковый номер через точку. " +
                "Порядковый номер участника можно увидеть в списке участников.";

            var telegramMessageModel = new TelegramMessageModel()
            {
                FromId = owner.Me!.TelegramUser!.ChatId,
                ToId = owner.Me!.TelegramUser!.ChatId,
                Content = text
            };

            await SendMessage(telegramMessageModel);
        }

        private async Task SendTutorialMessageParticipant(ParticipantModel participant)
        {
            string text = "Для того, чтобы отправить сообщение - просто введите текст и отправьте боту.";

            var telegramMessageModel = new TelegramMessageModel()
            {
                FromId = participant.Me!.TelegramUser!.ChatId,
                ToId = participant.Me!.TelegramUser!.ChatId,
                Content = text
            };

            await SendMessage(telegramMessageModel);
        }

        private async Task SendInfoAboutGroup(GroupAllocator.DAL.Entities.User model)
        {
            if (model == null || model.TelegramUser == null || model.TelegramUser.Group == null)
            {
                return;
            }

            var nameGroup = model.TelegramUser.Group.Name;
            var descGroup = model.TelegramUser.Group.Description;

            string resultMessageContent = $"Имя группы: {nameGroup}\nОписание: {descGroup}";

            var telegramMessageModel = new TelegramMessageModel
            {
                FromId = model.TelegramUser.ChatId,
                ToId = model.TelegramUser.ChatId,
                Content = resultMessageContent,
            };

            await SendMessage(telegramMessageModel);
        }

        private async Task SendListParticipants(OwnerModel model)
        {
            if (model == null || model.Me == null || model.Me.TelegramUser == null || model.Participants == null)
            {
                return;
            }

            StringBuilder resultMessageContent = new StringBuilder($"Участники:\n");

            foreach (var participant in model.Participants)
            {
                resultMessageContent.AppendLine("---");
                resultMessageContent.AppendLine($"Порядковый номер: {participant.Value.OrderNumber}");
                resultMessageContent.AppendLine($"Id: {participant.Key}");
                resultMessageContent.AppendLine($"Участник: {participant.Value.Me!.FirstName} {participant.Value.Me.LastName}");
                resultMessageContent.AppendLine($"Username: {participant.Value.Me!.TelegramUser!.Username}");
                resultMessageContent.AppendLine($"Telegram First name: {participant.Value.Me.TelegramUser.FirstName}");
                resultMessageContent.AppendLine($"Telegram Last name: {participant.Value.Me.TelegramUser.LastName}");
                resultMessageContent.AppendLine("---");
            }

            var telegramMessageModel = new TelegramMessageModel
            {
                FromId = model.Me.TelegramUser.ChatId,
                ToId = model.Me.TelegramUser.ChatId,
                Content = resultMessageContent.ToString(),
            };

            await SendMessage(telegramMessageModel);
        }

        private async Task SendMessageToParticipant(OwnerModel owner, string content)
        {
            var formatStr = content.Trim();
            
            long toId = owner.Me!.TelegramUser!.ChatId;
            var resultMessage = content;

            var indexPoint = formatStr.IndexOf('.');
            if (indexPoint == -1)
            {
                resultMessage = "Не найден порядковый номер участника.";
            }
            var orderNumberStr = formatStr.Substring(0, indexPoint);
            if (!int.TryParse(orderNumberStr, out int orderNumber))
            {
                resultMessage = "Не найден порядковый номер участника.";
            }
            else
            {
                if (owner.Participants.ContainsKey(orderNumber))
                {
                    toId = owner.Participants[orderNumber]!.Me!.TelegramUser!.ChatId;
                }
                else
                {
                    resultMessage = "Не найден порядковый номер участника.";
                }
            }
            await SendMessage(new TelegramMessageModel
            {
                FromId = owner.Me!.TelegramUser!.ChatId,
                ToId = toId,
                Content = resultMessage
            });
        }

        public async Task SendMessage(TelegramMessageModel message)
        {
            await _telegramBotClient
                .SendMessage(new Telegram.Bot.Types.ChatId(message.ToId), 
                message.Content, 
                cancellationToken: _cancellationToken.Token);
        }
    }
}
