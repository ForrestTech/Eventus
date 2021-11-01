namespace Eventus.IntegrationTests
{
    using Factories;
    using System;

    public class IntegrationTestsFixture : IDisposable
    {
        public IntegrationTestsFixture()
        {
            // Do "global" initialization here; Only called once.
        }

        public void Dispose()
        {
            ProviderFactory.Teardown().Wait();
        }
    }
}