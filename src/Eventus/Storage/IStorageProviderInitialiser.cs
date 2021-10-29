using System.Threading.Tasks;

namespace Eventus.Storage
{
    public interface IStorageProviderInitialiser
    {
        Task Init();

        void InitSync();
    }
}