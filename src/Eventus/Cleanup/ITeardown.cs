using System.Threading.Tasks;

namespace Eventus.Cleanup
{
    public interface ITeardown
    {
        Task TearDownAsync();
    }
}