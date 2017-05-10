using System;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure;
using Xunit;

namespace EventSourcing.Tests.Integration
{
    public class DocumentDbFixture : IDisposable
    {
        public DocumentDbFixture()
        {
            Setup().Wait();
        }

        private static async Task Setup()
        {
            var cleaner = TearDownFactory.Create();
            await cleaner.TearDownAsync();
        }

        public void Dispose()
        {
            //maybe run teardown again here.
        }
    }

    [CollectionDefinition(Name)]
    public class DocumetDbCollection : ICollectionFixture<DocumentDbFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.

        public const string Name = "DocumentDb collection";
    }
}