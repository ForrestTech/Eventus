namespace Eventus.UnitTests.Fixture
{
    using Events;
    using System;

    public class NonSnapshotableCreatedEvent : Event
    {
        public string TestValue { get; }

        public NonSnapshotableCreatedEvent(Guid aggregateId, int currentVersion, string testValue) : base(aggregateId, currentVersion)
        {
            TestValue = testValue;
        }
    }
}