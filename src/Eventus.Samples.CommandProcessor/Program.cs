using Serilog;
using Topshelf;

namespace Eventus.Samples.CommandProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Logger = configuration;

            HostFactory.Run(x =>                                 
            {
                x.Service<CommandProcessor>(s =>
                {
                    s.ConstructUsing(name => new CommandProcessor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.UseSerilog(configuration);
                x.SetDescription("Eventus Command Processor Service");
                x.SetDisplayName("Eventus Command Processor");
                x.SetServiceName("Eventus");
            });
        }
    }
}
