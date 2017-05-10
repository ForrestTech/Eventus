using System.Threading.Tasks;

namespace EventSourcing.Cleanup
{
    public interface ITeardown
    {
        Task TearDownAsync();
    }
}