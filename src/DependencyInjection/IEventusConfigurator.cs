namespace Eventus.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    public interface IEventusConfigurator
    {
        IServiceCollection Collection { get; } 
    }
}