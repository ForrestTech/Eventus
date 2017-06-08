using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Eventus.Samples.Core.ReadModel
{
    public class BankAccountReadModelRepository : IBankAccountReadModelRepository
    {
        private readonly ObjectCache _cache = MemoryCache.Default;
        private const string CacheKey = "BankAccount-";

        public BankAccountReadModelRepository()
        {
            var id = Guid.Parse("a7828c49-43d4-4d98-84e4-c34dad25566c");
            var key = CacheKey + id;
            var cip = new CacheItemPolicy
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(20))
            };
            if (!_cache.Contains(key))
            {
                _cache.Set(key, new BankAccountSummary {Id = id, Name = "Joe Bloggs", Balance = 100}, cip);
            }
        }

        public Task SaveAsync(BankAccountSummary account)
        {
            return Task.Run(() =>
            {
                var key = CacheKey + account.Id;
                var cip = new CacheItemPolicy
                {
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(20))
                };
                _cache.Set(key, account, cip);
            });
        }

        public Task<BankAccountSummary> GetAsync(Guid id)
        {
            var key = CacheKey + id;

            if (_cache.Contains(key))
            {
                return Task.FromResult((BankAccountSummary)_cache.Get(key));
            }
            return Task.FromResult<BankAccountSummary>(null);
        }
    }
}