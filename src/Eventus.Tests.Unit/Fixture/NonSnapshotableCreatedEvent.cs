using System;
using Eventus.Events;

namespace Eventus.Tests.Unit.Fixture
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