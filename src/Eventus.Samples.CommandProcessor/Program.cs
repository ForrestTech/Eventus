using Topshelf;

namespace Eventus.Samples.CommandProcessor
{
    //todo add serilog and seq to command processor
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 
            {
                x.Service<CommandProcessor>(s =>
                {
                    s.ConstructUsing(name => new CommandProcessor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Eventus Command Processor Service");
                x.SetDisplayName("Eventus Command Processor");
                x.SetServiceName("Eventus");
            });
        }
    }
}
