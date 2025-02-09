using GroupAllocator.Models.GroupModels;
using GroupAllocator.Models.UserModels;

namespace GroupAllocator.Models.TelegramModels
{
    public class TelegramUserModel
    {
        public long ChatId { get; set; } // from telegram

        public string PhoneNumber { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public UserRoleModel UserRole { get; set; } = new UserRoleModel();

        public GroupModel? Group { get; set; } = new GroupModel();
    }
}
