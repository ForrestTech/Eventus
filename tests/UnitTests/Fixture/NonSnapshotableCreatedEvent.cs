namespace Eventus.UnitTests.Fixture
{
    using Events;
    using System;

    public class NonSnapshotableCreatedEvent : Event
    {
        public string TestValue { get; }

        public NonSnapshotableCreatedEvent(Guid id, int currentVersion, string testValue) : base(id, currentVersion)
        {
            TestValue = testValue;
        }
    }
}