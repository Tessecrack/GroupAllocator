using GroupAllocator.DAL.Entities;
using System.Collections.Concurrent;

namespace GroupAllocator.TelegramBotService.Models
{
    internal class OwnerModel
    {
        public User? Me { get; set; }

        public ConcurrentDictionary<long, ParticipantModel> Participants { get; set; } = new();
    }
}