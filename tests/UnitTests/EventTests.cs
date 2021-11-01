using System;
using System.Text.Json;
using Eventus.Samples.Core.Domain;
using FluentAssertions;
using MassTransit;
using Xunit;

namespace Eventus.UnitTests
{
    public class EventTests 
    {
        [Fact]
        public void Event_can_be_serialized()
        {
            var deposit = new FundsWithdrawalEvent(NewId.NextGuid(), 1, 100);

            var data = JsonSerializer.Serialize(deposit);

            var actual = JsonSerializer.Deserialize<FundsWithdrawalEvent>(data);

            actual.Should().BeEquivalentTo(deposit);
        }
        
        [Fact]
        public void Event_with_multiple_constructor_can_be_serialized_when_using_constructor_attribute()
        {
            var deposit = new FundsDepositedEvent(NewId.NextGuid(), 1, 100);

            var data = JsonSerializer.Serialize(deposit);

            var actual = JsonSerializer.Deserialize<FundsDepositedEvent>(data);

            actual.Should().BeEquivalentTo(deposit);
        }
    }
}
