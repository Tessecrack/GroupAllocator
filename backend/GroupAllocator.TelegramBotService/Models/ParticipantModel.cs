using GroupAllocator.DAL.Entities;

namespace GroupAllocator.TelegramBotService.Models
{
    internal class ParticipantModel
    {
        public int OrderNumber { get; set; }

        public User? Me { get; set; }

        public User? Owner { get; set; }
    }
}