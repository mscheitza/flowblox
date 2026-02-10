using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using System.Collections.Concurrent;

namespace FlowBlox.Core.Authentication
{
    public class FlowBloxAccountManager
    {
        private static readonly Lazy<FlowBloxAccountManager> _instance =
            new Lazy<FlowBloxAccountManager>(() => new FlowBloxAccountManager());

        private readonly ConcurrentDictionary<string, FbUserData> _activeUsersByApiUrl =
            new ConcurrentDictionary<string, FbUserData>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, string> _tokensByApiUrl =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private FlowBloxAccountManager()
        {
        }

        public static FlowBloxAccountManager Instance => _instance.Value;

        public bool IsLoggedInFor(string apiUrl) => GetActiveUser(apiUrl) != null;

        public FbUserData GetActiveUser(string apiUrl)
        {
            var key = FlowBloxApiUrlNomalizer.Normalize(apiUrl);
            _activeUsersByApiUrl.TryGetValue(key, out var user);
            return user;
        }

        public string GetUserToken(string apiUrl)
        {
            var key = FlowBloxApiUrlNomalizer.Normalize(apiUrl);
            _tokensByApiUrl.TryGetValue(key, out var token);
            return token;
        }

        public void SetSession(string apiUrl, FbUserData user, string token)
        {
            var key = FlowBloxApiUrlNomalizer.Normalize(apiUrl);

            if (user == null)
                _activeUsersByApiUrl.TryRemove(key, out _);
            else
                _activeUsersByApiUrl[key] = user;

            if (string.IsNullOrWhiteSpace(token))
                _tokensByApiUrl.TryRemove(key, out _);
            else
                _tokensByApiUrl[key] = token;
        }

        public void SetActiveUser(string apiUrl, FbUserData user)
        {
            var key = FlowBloxApiUrlNomalizer.Normalize(apiUrl);

            if (user == null)
                _activeUsersByApiUrl.TryRemove(key, out _);
            else
                _activeUsersByApiUrl[key] = user;
        }

        public void SetUserToken(string apiUrl, string token)
        {
            var key = FlowBloxApiUrlNomalizer.Normalize(apiUrl);

            if (string.IsNullOrWhiteSpace(token))
                _tokensByApiUrl.TryRemove(key, out _);
            else
                _tokensByApiUrl[key] = token;
        }
    }
}
