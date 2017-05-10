using System.Threading.Tasks;
using EventSourceDemo.Commands;

namespace EventSourceDemo
{
    internal interface IHandleCommands<in T> where T : ICommand
    {
        Task HandleAsync(T command);
    }
}