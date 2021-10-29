﻿namespace Console.SqlServer
{
    using Eventus.Extensions.DependencyInjection;
    using Eventus.Samples.Core;
    using Eventus.SqlServer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            //start mssql server with this docker command: sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<YourStrong@Passw0rd>" -p 1433:1433 --name sql1 -h sql1 -d mcr.microsoft.com/mssql/server:2019-latest
            
            const string connectionString =
                "Server=127.0.0.1,1433;Database=Eventus;User Id=sa;Password=yourStrong(!)Password;";

            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEventus(options =>
            {
                options.SnapshotOptions.SnapshotFrequency = 3;
            }).UseSqlServer(connectionString);

            services.AddSingleton(new SqlServerTeardown(connectionString));

            var serviceProvider = services.BuildServiceProvider();
            await SampleLogic.Run(serviceProvider);
        }
    }
}