using System;
using System.Runtime.Serialization;

namespace App.Exceptions
{
    [Serializable]
    public class AppConfigurationException : ApplicationException
    {
        protected AppConfigurationException()
        {
        }

        protected AppConfigurationException(string message) : base(message)
        {
        }

        protected AppConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AppConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static AppConfigurationException KeyNotFound(string key)
        {
            return new AppConfigurationException($"Key '{key}' is not found.");
        }
    }
}
