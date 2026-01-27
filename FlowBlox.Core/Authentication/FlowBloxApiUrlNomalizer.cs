namespace FlowBlox.Core.Authentication
{
    public static class FlowBloxApiUrlNomalizer
    {
        public static string Normalize(string apiUrl)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
                return apiUrl;

            var normalized = apiUrl.Trim();
            return normalized.EndsWith("/") ? 
                normalized.TrimEnd('/') : 
                normalized;
        }
    }
}
