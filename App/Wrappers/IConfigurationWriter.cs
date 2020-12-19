using System.Threading;
using System.Threading.Tasks;

namespace App.Wrappers
{
    public interface IConfigurationWriter
    {
        Task WriteKeyValueAsync(
            string key,
            string value,
            string label = default,
            bool overwriteIfExists = true,
            CancellationToken stoppingToken = default);

        Task DeleteKeyValueAsync(string key, string label = default, CancellationToken stoppingToken = default);
    }
}
