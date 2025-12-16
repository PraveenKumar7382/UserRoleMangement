using NUnit.Framework;
using UserRoleMangement.TokenGeneration;

namespace UnitTestCases.Tokengeneration
{
    public class StoreTokenInMemoryTests
    {
        [Test]
        public void AddToken_ForUser()
        {
            var store = new StoreTokenInMemory();
            int userId = 1;
            string token = "token123";

            store.AddToken(userId, token);

            Assert.That(store.IsValidToken(userId, token));
        }

        [Test]
        public void IsValidToken__ReturnsFalse()
        {
            var store = new StoreTokenInMemory();

            var result = store.IsValidToken(1, "invalid");

            Assert.That(result, Is.False);
        }

        [Test]
        public void RemoveToken_ShouldRemoveOnlyThatToken()
        {
            var store = new StoreTokenInMemory();
            int userId = 1;

            var token1 = "token1";
            var token2 = "token2";

            store.AddToken(userId, token1);

            store.RemoveToken(userId);

            Assert.That(store.IsValidToken(userId, token2), Is.False);
        }

        [Test]
        public void RemoveToken_UserEntryIsDeleted()
        {
            var store = new StoreTokenInMemory();
            int userId = 1;
            string token = "token123";

            store.AddToken(userId, token);
            store.RemoveToken(userId);

            Assert.False(store.IsValidToken(userId, token));
        }
    }
}
