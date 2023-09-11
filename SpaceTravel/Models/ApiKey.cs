namespace SpaceTravel.Models
{
    public class ApiKey
    {
        public string Key { get; set; }
        public string User { get; set; }
        public int RequestLimit { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class ApiKeyManager
    {
        private static Dictionary<string, ApiKey> apiKeys = new Dictionary<string, ApiKey>();

        public static bool IsApiKeyValid(string key)
        {
            if (apiKeys.TryGetValue(key, out var apiKey))
            {
                if (apiKey.ExpiryDate < DateTime.UtcNow)
                {
                    return false; 
                }
                return true;
            }
            return false; 
        }
    }

}
