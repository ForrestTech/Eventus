using System.Threading.Tasks;
using Eventus.Samples.Core.Commands;

namespace Eventus.Samples.Core
{
    internal interface IHandleCommands<in T> where T : ICommand
    {
        Task HandleAsync(T command);
    }
}