using System;
using System.Threading.Tasks;
using MediatR;
using ServiceStack.Redis;
using Eventus.Samples.Contracts.BankAccount;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Get
    {
        public class Query : IRequest<BankAccountSummary>
        {
            public Guid AccountId { get; set; }
        }

        public class Handler : IAsyncRequestHandler<Query, BankAccountSummary>
        {
            private readonly RedisManagerPool _redisManagerPool;

            public Handler()
            {
                //todo move to config
                _redisManagerPool = new RedisManagerPool("Password1@redis-15191.c1.eu-west-1-3.ec2.cloud.redislabs.com:15191");
            }

            public Task<BankAccountSummary> Handle(Query message)
            {
                using (var client = _redisManagerPool.GetClient())
                {
                    var accountSummaryClient = client.As<BankAccountSummary>();

                    var account = accountSummaryClient.GetById(message.AccountId);

                    return Task.FromResult(account);
                }
            }
        }
    }
}