using System;
using System.Threading.Tasks;
using EasyNetQ;
using Eventus.EventBus;
using Eventus.Events;
using Eventus.Samples.Core.Events;

namespace Eventus.Samples.CommandProcessor
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly IBus _bus;

        public RabbitMqEventPublisher(IBus bus)
        {
            _bus = bus;
        }

        public Task PublishAsync(IEvent @event)
        {
            //the way EasyNetQ handles publish subscribe means that you have to emit the type event not an interface. 
            switch (@event.GetType().Name)
            {
                case nameof(AccountCreatedEvent):
                    return PublishAsync((AccountCreatedEvent)@event);
                case nameof(FundsDepositedEvent):
                    return PublishAsync((FundsDepositedEvent)@event);
                case nameof(FundsWithdrawalEvent):
                    return PublishAsync((FundsWithdrawalEvent)@event);
                default:
                    throw new InvalidOperationException("No handler found for event of type: " + nameof(@event));
            }
        }

        private Task PublishAsync(AccountCreatedEvent @event)
        {
            return _bus.PublishAsync(@event);
        }

        private Task PublishAsync(FundsDepositedEvent @event)
        {
            return _bus.PublishAsync(@event);
        }

        private Task PublishAsync(FundsWithdrawalEvent @event)
        {
            return _bus.PublishAsync(@event);
        }
    }
}