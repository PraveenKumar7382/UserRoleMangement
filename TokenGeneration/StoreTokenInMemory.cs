using System.Collections.Concurrent;

namespace UserRoleMangement.TokenGeneration
{
    public class StoreTokenInMemory : IUserTokenStore
    {
        private readonly ConcurrentDictionary<int, HashSet<string>> _userTokens = new();

        public void AddToken(int userId, string token)
        {
            var tokens = _userTokens.GetOrAdd(userId, _ => new HashSet<string>());
            lock (tokens)
            {
                tokens.Add(token);
            }
        }

        public bool IsValidToken(int userId, string token)
        {
            return _userTokens.TryGetValue(userId, out var tokens) && tokens.Contains(token);
        }

        public void RemoveToken(int userId, string token)
        {
            if (_userTokens.TryGetValue(userId, out var tokens))
            {
                lock (tokens)
                {
                    tokens.Remove(token);
                }
            }
        }
    }
}
