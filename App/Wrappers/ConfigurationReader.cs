using App.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace App.Wrappers
{
    public class ConfigurationReader : IConfigurationReader
    {
        private readonly IOptions<Settings> _options;
        private readonly IConfiguration _configuration;

        public ConfigurationReader(IOptions<Settings> options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public bool IsKeyExists(string key)
        {
            return _options.Value.Keys.SingleOrDefault(k => key == k) != null;
        }

        public IDictionary<string, string> ReadKeysValues()
        {
            return _options.Value.Keys.ToDictionary(x => x, x => SafeReadKeyValue(x));
        }

        public string ReadKeyValue(string key)
        {
            if (!IsKeyExists(key))
            {
                throw AppConfigurationException.KeyNotFound(key);
            }

            return _configuration[key];
        }

        private string SafeReadKeyValue(string key)
        {
            return IsKeyExists(key) ? _configuration[key] : null;
        }
    }
}
