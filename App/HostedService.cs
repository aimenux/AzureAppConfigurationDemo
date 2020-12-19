using App.Wrappers;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    public class HostedService : BackgroundService
    {
        private readonly TimeSpan _delay;
        private readonly IOptions<Settings> _options;
        private readonly IConfigurationReader _configurationReader;
        private readonly IConfigurationWriter _configurationWriter;
        private readonly IConfigurationRefresher _configurationRefresher;
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        public HostedService(
            IOptions<Settings> options,
            IConfigurationReader configurationReader,
            IConfigurationWriter configurationWriter,
            IConfigurationRefresher configurationRefresher)
        {
            _options = options;
            _configurationReader = configurationReader;
            _configurationWriter = configurationWriter;
            _configurationRefresher = configurationRefresher;
            _delay = TimeSpan.FromSeconds(_options.Value.ServiceDelayInSeconds);
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            ConsoleColor.Green.WriteLine("Writing keys/values :");

            foreach (var key in _options.Value.Keys)
            {
                var value = Random.Next().ToString();
                ConsoleColor.Yellow.WriteLine($"\t Key = {key} --> Value = {value}");
                await _configurationWriter.WriteKeyValueAsync(key, value, stoppingToken: stoppingToken);
            }

            await base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                ConsoleColor.Green.WriteLine("Reading keys/values :");

                await _configurationRefresher.RefreshAsync();

                var keysValues = _configurationReader.ReadKeysValues();
                foreach (var (key, value) in keysValues)
                {
                    ConsoleColor.Yellow.WriteLine($"\t Key = {key} --> Value = {value}");
                }

                await Task.Delay(_delay, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            ConsoleColor.Green.WriteLine("Deleting keys/values :");

            foreach (var key in _options.Value.Keys)
            {
                var value = _configurationReader.ReadKeyValue(key);
                ConsoleColor.Yellow.WriteLine($"\t Key = {key} --> Value = {value}");
                await _configurationWriter.DeleteKeyValueAsync(key, stoppingToken: stoppingToken);
            }

            await base.StopAsync(stoppingToken);
        }
    }
}
