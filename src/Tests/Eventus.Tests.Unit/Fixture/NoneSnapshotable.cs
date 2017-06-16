using Eventus.Domain;

namespace Eventus.Tests.Unit.Fixture
{
    public class NoneSnapshotable : Aggregate
    {
        public NoneSnapshotable()
        { }

        public NoneSnapshotable(string testValue)
        {
            var created = new NonSnapshotableCreatedEvent(Id, CurrentVersion, testValue);
            ApplyEvent(created);
        }

        private void OnCreated(NonSnapshotableCreatedEvent @event)
        {
            TestValue = @event.TestValue;
        }

        public string TestValue { get; set; }
    }
}