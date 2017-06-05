using System;
using Eventus.Samples.Core.Commands;
using FluentAssertions;
using Xunit;

namespace Eventus.Tests.Unit
{
    public class CommandTests
    {
        [Fact]
        public void Commands_can_be_constructed()
        {
            var depositFundsCommand = new DepositFundsCommand(Guid.NewGuid(), Guid.NewGuid(), 100);
        }

        [Fact]
        public void Commands_cant_have_empty_correlation_tokens()
        {
            Action act = () =>
            {
                var command = new DepositFundsCommand(Guid.Empty, Guid.NewGuid(), 100);
            };

            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Commands_cant_have_empty_aggregate_id()
        {
            Action act = () =>
            {
                var command = new DepositFundsCommand(Guid.NewGuid(), Guid.Empty, 100);
            };

            act.ShouldThrow<ArgumentException>();
        }
    }
}