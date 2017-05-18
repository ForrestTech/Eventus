namespace Eventus.Storage
{
    public interface ISnapshottable
    {
        Snapshot TakeSnapshot();

        void ApplySnapshot(Snapshot snapshot);
    }
}