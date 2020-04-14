using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Conventions;
using MovieBot.Worker.DataAccess;
using MovieBot.Worker.Services;
using NLog.Web;

namespace MovieBot.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("init main");
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    var pack = new ConventionPack();
                    pack.Add(new CamelCaseElementNameConvention());
                    ConventionRegistry.Register("camel case", pack, t => true);

                    services.AddHostedService<Worker>();

                    services.AddSingleton<IMovieBotDbFactory, MovieBotDbFactory>();
                    services.AddTransient<IPollDataAccess, PollDataAccess>();
                    services.AddTransient<IBotCommandsService, BotCommandsService>();
                    services.AddTransient<IBotCommandsService, BotCommandsService>();
                    services.AddTransient<IPollService, PollService>();
                });
    }
}