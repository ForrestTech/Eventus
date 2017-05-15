using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.EventBus;
using EventSourcing.Events;
using EventSourcing.Samples.Core.EventHandlers;
using EventSourcing.Samples.Core.Events;

namespace EventSourcing.Samples.Core
{
    public class DemoPublisher : IEventPublisher
    {
        private readonly Dictionary<Type, Action<IEvent>> _switch;

        public DemoPublisher(IHandleEvent<FundsDepositedEvent> handleDeposit, IHandleEvent<FundsWithdrawalEvent> handleWithdrawal)
        {
            _switch = new Dictionary<Type, Action<IEvent>>
            {
                {
                    typeof(FundsDepositedEvent), x => { handleDeposit.Handle((FundsDepositedEvent) x); }
                },
                {
                    typeof(FundsWithdrawalEvent), x => { handleWithdrawal.Handle((FundsWithdrawalEvent) x); }
                }
            };
        }

        public Task PublishAsync(IEvent @event)
        {
            return Task.Run(() =>
            {
                if(_switch.ContainsKey(@event.GetType()))
                    _switch[@event.GetType()](@event);
            });
        }
    }
}