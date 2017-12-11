namespace CodingMonkey.CodeExecutor
{
    using System.IO;

    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.secrets.json", optional: false);
                })
                .Build();
    }
}
