using System;
using System.Collections.Generic;

namespace App
{
    public class Settings
    {
        public string ConnectionString { get; set; }

        public int ServiceDelayInSeconds { get; set; }

        public int CacheExpirationInSeconds { get; set; }

        public TimeSpan CacheExpiration => TimeSpan.FromSeconds(CacheExpirationInSeconds);

        public ICollection<string> Keys { get; set; }
    }
}
