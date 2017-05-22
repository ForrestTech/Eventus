using System;
using System.Threading.Tasks;
using Eventus.Samples.Infrastructure.Factories;
using Xunit;

namespace EventSourcing.Tests.Integration
{
    public class StorageProviderFixture : IDisposable
    {
        public StorageProviderFixture()
        {
            SetupAsync().Wait();
        }

        private static Task SetupAsync()
        {
            return StorageProviderInitialiser.InitAsync();
        }

        public void Dispose()
        {
            var cleaner = TearDownFactory.CreateAsync().Result;
            cleaner.TearDownAsync().Wait();
        }
    }

    [CollectionDefinition(Name)]
    public class StorageProvidersCollection : ICollectionFixture<StorageProviderFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.

        public const string Name = "DocumentDb collection";
    }
}