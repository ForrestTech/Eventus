namespace Eventus.Storage
{
    /// <summary>
    /// Defines an aggregate as Snapshottable.  If this is applied to an aggregate then snapshots are taken at set frequencies
    /// when storing the aggregate. Snapshotable aggregates are loaded from the latest snapshot first   
    /// </summary>
    public interface ISnapshottable
    {
        /// <summary>
        /// Take a Snapshot that can be used to load the state of the aggregate
        /// </summary>
        /// <returns>The Snapshot to store</returns>
        Snapshot TakeSnapshot();

        /// <summary>
        /// Apply the stored Snapshot to the aggregate to set its state to the point when the Snapshot was taken
        /// </summary>
        /// <param name="snapshot">The Snapshot to apply</param>
        void ApplySnapshot(Snapshot? snapshot);
    }
}