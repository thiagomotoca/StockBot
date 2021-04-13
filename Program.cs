using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using StockBot.Extensions;
using StockBot.Interfaces.Services;
using StockBot.Model;
using StockBot.Services;

namespace StockBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();

                    IConfiguration configuration = hostContext.Configuration;
                    var workerOptions = configuration.GetSection("WorkerOptions").Get<WorkerOptions>();
                    services.AddSingleton(workerOptions);

                    services.AddTransient<IYahooService, YahooService>();
                    services.AddTransient<ITwitterService, TwitterService>();
                    
                    services.AddQuartz(q =>  
                    {
                        q.UseMicrosoftDependencyInjectionScopedJobFactory();
                        q.AddJobAndTrigger<Worker>(hostContext.Configuration);
                    });

                    services.AddQuartzHostedService(
                        q => q.WaitForJobsToComplete = true);
                });
    }
}
