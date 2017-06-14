using System;
using System.Threading.Tasks;
using MediatR;
using ServiceStack.Redis;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Get
    {
        public class Query : IRequest<BankAccountSummary>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IAsyncRequestHandler<Query, BankAccountSummary>
        {
            private readonly RedisManagerPool _redisManagerPool;

            public Handler()
            {
                _redisManagerPool = new RedisManagerPool("localhost:6379");
            }

            public Task<BankAccountSummary> Handle(Query message)
            {
                using (var client = _redisManagerPool.GetClient())
                {
                    var accountSummaryClient = client.As<BankAccountSummary>();

                    var account = accountSummaryClient.GetById(message.Id);

                    return Task.FromResult(account);
                }
            }
        }

        public class BankAccountSummary
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public decimal Balance { get; set; }
        }
    }
}