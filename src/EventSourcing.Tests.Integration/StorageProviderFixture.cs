using System;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure;
using EventSourcing.Samples.Infrastructure.Factories;
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
            var cleaner = TearDownFactory.Create();
            return cleaner.TearDownAsync();
        }

        public void Dispose()
        {
            //maybe run teardown again here.
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