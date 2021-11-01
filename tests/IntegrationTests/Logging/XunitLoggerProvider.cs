namespace Eventus.IntegrationTests.Logging
{
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
            => new XunitLogger(_testOutputHelper, categoryName);

        public void Dispose()
        { }
    }
}