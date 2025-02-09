namespace GroupAllocator.Models.ServiceModels
{
    public class GroupAllocatorClientOptions
    {
        public string AccessToken { get; }

        public GroupAllocatorClientOptions(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}