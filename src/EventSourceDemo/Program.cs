using System;
using System.Collections.Generic;
using System.Net;
using EventStore.ClientAPI;

namespace EventSourceDemo
{
    class Program
    {
        //todo
        //create commands
        //create handlers
        //create validators
        //snapshots
        //subscriptions
        //readmodel
        private static string StreamId(Guid id)
        {
            return $"bankAccount-{id}";
        }

        static void Main(string[] args)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();

            var aggregateId = Guid.NewGuid();
            var eventsToRun = new List<Event>
            {
                new AccountCreatedEvent(aggregateId, "Joe Doe " + Guid.NewGuid()),
                new FundsDepositedEvent(aggregateId, 150),
                new FundsDepositedEvent(aggregateId, 100),
                new FundsWithdrawalEvent(aggregateId, 60),
                new FundsWithdrawalEvent(aggregateId, 94),
                new FundsDepositedEvent(aggregateId, 4)
            };

            var wallet = new Wallet();

            foreach (var e in eventsToRun)
            {
                wallet.ApplyEvent(e);
            }

            var repo = new EventStoreRepository(connection);
            repo.Save(wallet);

            var fromStore = repo.GetById<Wallet>(wallet.Id);
            
            Console.ReadLine();
        }
    }

    
}
