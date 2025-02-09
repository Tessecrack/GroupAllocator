namespace GroupAllocator.DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string? FirstName { get; set; } // real first name

        public string? LastName { get; set; } // real last name

        public Guid TelegramUserId { get; set; }

        public TelegramUser? TelegramUser { get; set; }
    }
}