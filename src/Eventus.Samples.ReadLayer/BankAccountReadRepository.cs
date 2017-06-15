using System;
using Eventus.Samples.Contracts.BankAccount;
using ServiceStack.Redis;

namespace Eventus.Samples.ReadLayer
{
    public class BankAccountReadRepository
    {
        private readonly RedisManagerPool _redisManagerPool;

        public BankAccountReadRepository(string connectionString)
        {
            _redisManagerPool = new RedisManagerPool(connectionString);
        }

        public void Save(BankAccountSummary summary)
        {
            using (var client = _redisManagerPool.GetClient())
            {
                var accountSummaryClient = client.As<BankAccountSummary>();

                accountSummaryClient.Store(summary);
            }
        }

        public BankAccountSummary Get(Guid id)
        {
            using (var client = _redisManagerPool.GetClient())
            {
                var accountSummaryClient = client.As<BankAccountSummary>();

                var account = accountSummaryClient.GetById(id);

                return account;
            }
        }
    }
}
