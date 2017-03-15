using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EventSourceDemo.ReadModel
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly ObjectCache _cache = MemoryCache.Default;
        private const string CacheKey = "TopAccounts";

        public Task Save(TopAccountsReadModel model)
        {
            return Task.Run(() =>
            {
                var cip = new CacheItemPolicy
                {
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(20))
                };
                _cache.Set(CacheKey, model, cip);
            });
        }

        public Task<TopAccountsReadModel> Get()
        {
            if (_cache.Contains(CacheKey))
            {
                return Task.FromResult((TopAccountsReadModel) _cache.Get(CacheKey));
            }
            return Task.FromResult<TopAccountsReadModel>(null);
        }
    }
}