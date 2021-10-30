namespace Eventus.Storage
{
    using Configuration;
    using Domain;

    public class SnapshotCalculator : ISnapshotCalculator 
    {
        private readonly EventusOptions _options;

        public SnapshotCalculator(EventusOptions options)
        {
            _options = options;
        }
        
        public bool ShouldCreateSnapShot(Aggregate aggregate)
        {
            var snapshotFrequency = _options.GetSnapshotFrequency(aggregate.GetType());
            var currentVersion = aggregate.CurrentVersion;
            var numberOfChanges = aggregate.GetUncommittedChanges().Count;
            
            return currentVersion >= snapshotFrequency &&
                   (
                       numberOfChanges >= snapshotFrequency ||
                       currentVersion % snapshotFrequency < numberOfChanges ||
                       currentVersion % snapshotFrequency == 0
                   );
        }
    }
}