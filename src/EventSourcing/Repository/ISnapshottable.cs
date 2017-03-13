namespace EventSourcing.Repository
{
    public interface ISnapshottable
    {
        Snapshot TakeSnapshot();

        void ApplySnapshot(Snapshot snapshot);
    }
}