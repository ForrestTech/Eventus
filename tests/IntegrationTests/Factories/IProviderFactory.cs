namespace Eventus.IntegrationTests.Factories
{
    using Storage;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public interface IProviderFactory
    {
        string Key { get; }
        
        IEventStorageProvider GetStorageProvider(ITestOutputHelper output);
        
        ISnapshotStorageProvider GetSnapshotProvider(ITestOutputHelper output);
        
        Task Teardown();
    }
}