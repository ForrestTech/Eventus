using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Eventus.Samples.Core.ReadModel
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly ObjectCache _cache = MemoryCache.Default;
        private const string CacheKey = "TopAccounts";

        public Task SaveAsync(TopAccountsReadModel model)
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

        public Task<TopAccountsReadModel> GetAsync()
        {
            if (_cache.Contains(CacheKey))
            {
                return Task.FromResult((TopAccountsReadModel) _cache.Get(CacheKey));
            }
            return Task.FromResult<TopAccountsReadModel>(null);
        }
    }
}