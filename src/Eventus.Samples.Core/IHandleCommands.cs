using System.Threading.Tasks;
using Eventus.Commands;

namespace Eventus.Samples.Core
{
    internal interface IHandleCommands<in T> where T : ICommand
    {
        Task HandleAsync(T command);
    }
}