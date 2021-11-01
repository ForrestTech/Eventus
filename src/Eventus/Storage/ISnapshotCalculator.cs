namespace Eventus.Storage
{
    using Domain;

    public interface ISnapshotCalculator
    {
        bool ShouldCreateSnapShot(Aggregate aggregate);
    }
}