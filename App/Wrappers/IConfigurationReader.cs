using System.Collections.Generic;

namespace App.Wrappers
{
    public interface IConfigurationReader
    {
        bool IsKeyExists(string key);
        string ReadKeyValue(string key);
        IDictionary<string, string> ReadKeysValues();
    }
}
