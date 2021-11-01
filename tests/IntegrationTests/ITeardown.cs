namespace Eventus.IntegrationTests
{
    using System.Threading.Tasks;

    public interface ITeardown
    {
        Task TearDownAsync();
    }
}