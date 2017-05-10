using System.Threading.Tasks;
using EventSourcing.Samples.Core.Commands;

namespace EventSourcing.Samples.Core
{
    internal interface IHandleCommands<in T> where T : ICommand
    {
        Task HandleAsync(T command);
    }
}