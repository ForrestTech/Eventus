namespace Eventus.Storage
{
    using Domain;
    using Events;
    using System.Collections.Generic;

    public interface ISnapshotCalculator
    {
        bool ShouldCreateSnapShot(Aggregate aggregate);
    }
}