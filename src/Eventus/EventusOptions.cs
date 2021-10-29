namespace Eventus
{
    public class EventusOptions
    {
        public SnapshotOptions SnapshotOptions { get; set; } = new SnapshotOptions();
    }

    public class SnapshotOptions
    {
        public SnapshotOptions()
        {
            SnapshotFrequency = 10;
        }

        public int SnapshotFrequency { get; set; }
    }
}