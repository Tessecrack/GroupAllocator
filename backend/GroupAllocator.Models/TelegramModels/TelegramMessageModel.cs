namespace GroupAllocator.Models.TelegramModels
{
    public class TelegramMessageModel
    {
        public long FromId { get; set; }

        public long ToId { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
