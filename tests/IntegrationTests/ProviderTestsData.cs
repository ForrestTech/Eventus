namespace Eventus.IntegrationTests
{
    using Factories;
    using System.Linq;
    using Xunit;

    public class ProviderTestsData : TheoryData<string>
    {
        public ProviderTestsData()
        {
            ProviderFactory.Providers
                .Select(x => new object[] {x})
                .ToList()
                .ForEach(AddRow);
        }
    }
}