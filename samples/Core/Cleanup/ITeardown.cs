namespace Eventus.Samples.Core.Cleanup
{
    using System.Threading.Tasks;

    public interface ITeardown
    {
        Task TearDownAsync();
    }
}