using System.ComponentModel.DataAnnotations;

namespace GroupAllocator.DAL.Entities
{
    public class TelegramUser
    {
        public Guid Id { get; set; }

        [Required]
        public long ChatId { get; set; } // from telegram

        public string? PhoneNumber { get; set; } // from telegram

        public string? Username { get; set; }   // from telegram

        public string? FirstName { get; set; }  // from telegram

        public string? LastName { get; set; }   // from telegram

        public Guid UserRoleId { get; set; }

        public UserRole? UserRole { get; set; }

        public Guid GroupId { get; set; }

        public Group? Group { get; set; }
    }
}