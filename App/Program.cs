using App.Wrappers;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace App
{
    public static class Program
    {
        private static IConfigurationRefresher ConfigurationRefresher { get; set; }

        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();

            Console.WriteLine("Press any key to exit !");
            Console.ReadKey();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddCommandLine(args);
                config.AddEnvironmentVariables();
                config.SetBasePath(Directory.GetCurrentDirectory());
                var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);
                var settings = config.Build().GetSection(nameof(Settings)).Get<Settings>();
                config.AddAzureAppConfiguration(configOptions =>
                {
                    configOptions.Connect(settings.ConnectionString);
                    configOptions.Select(KeyFilter.Any, environment);
                    configOptions.Select(KeyFilter.Any, LabelFilter.Null);

                    ConfigurationRefresher = configOptions.GetRefresher();

                    configOptions.ConfigureRefresh(refreshOptions =>
                    {
                        refreshOptions.SetCacheExpiration(settings.CacheExpiration);
                        foreach (var key in settings.Keys)
                        {
                            refreshOptions.Register(key, environment, true);
                            refreshOptions.Register(key, LabelFilter.Null, true);
                        }
                    });
                });
            })
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.AddConsoleLogger();
                loggingBuilder.AddNonGenericLogger();
                loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddOptions();
                services.AddHostedService<HostedService>();
                services.AddSingleton(ConfigurationRefresher);
                services.AddSingleton(hostingContext.Configuration);
                services.AddTransient<IConfigurationWriter>(serviceProvider =>
                {
                    var settings = serviceProvider.GetService<IOptions<Settings>>().Value;
                    var client = new ConfigurationClient(settings.ConnectionString);
                    return new ConfigurationWriter(client);
                });
                services.AddTransient<IConfigurationReader, ConfigurationReader>();
                services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
            });

        private static void AddConsoleLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "[HH:mm:ss:fff] ";
                options.ColorBehavior = LoggerColorBehavior.Enabled;
            });
        }

        private static void AddNonGenericLogger(this ILoggingBuilder loggingBuilder)
        {
            var services = loggingBuilder.Services;
            services.AddSingleton(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("AzureAppConfigurationDemo");
            });
        }
    }
}
