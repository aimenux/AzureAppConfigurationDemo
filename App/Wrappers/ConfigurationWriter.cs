using Azure.Data.AppConfiguration;
using System.Threading;
using System.Threading.Tasks;

namespace App.Wrappers
{
    public class ConfigurationWriter : IConfigurationWriter
    {
        private readonly ConfigurationClient _client;

        public ConfigurationWriter(ConfigurationClient client)
        {
            _client = client;
        }

        public Task WriteKeyValueAsync(
            string key,
            string value,
            string label = default,
            bool overwriteIfExists = true,
            CancellationToken stoppingToken = default)
        {
            const bool onlyIfUnchanged = false;
            var setting = new ConfigurationSetting(key, value, label);

            return overwriteIfExists
                ? _client.SetConfigurationSettingAsync(setting, onlyIfUnchanged, stoppingToken)
                : _client.AddConfigurationSettingAsync(setting, stoppingToken);
        }

        public Task DeleteKeyValueAsync(string key, string label = default, CancellationToken stoppingToken = default)
        {
            return _client.DeleteConfigurationSettingAsync(key, label, stoppingToken);
        }
    }
}
