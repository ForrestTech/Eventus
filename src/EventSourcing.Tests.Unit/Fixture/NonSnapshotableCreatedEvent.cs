using System;
using EventSourcing.Events;

namespace EventSourcing.Tests.Unit.Fixture
{
    public class NonSnapshotableCreatedEvent : Event
    {
        public string TestValue { get; }

        public NonSnapshotableCreatedEvent(Guid id, int currentVersion, string testValue) : base(id, currentVersion)
        {
            TestValue = testValue;
        }
    }
}