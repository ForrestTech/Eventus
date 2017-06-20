using Serilog;
using Topshelf;

namespace Eventus.Samples.Subscribers
{
    class Program
    {
        static void Main()
        {
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Logger = configuration;

            HostFactory.Run(x =>
            {
                x.Service<Subscribers>(s =>
                {
                    s.ConstructUsing(name => new Subscribers());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.UseSerilog(configuration);
                x.SetDescription("Subscribes to eventus samples domain events");
                x.SetDisplayName("Eventus Subscriber");
                x.SetServiceName("Eventus Subscriber");
            });
        }
    }
}
