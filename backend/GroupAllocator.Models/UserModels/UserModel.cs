using GroupAllocator.Models.TelegramModels;

namespace GroupAllocator.Models.UserModels
{
    public class UserModel
    {
        public string FirstName { get; set; } = string.Empty; // real first name

        public string LastName { get; set; } = string.Empty; // real last name

        public TelegramUserModel? TelegramUserModel { get; set; }
    }
}
