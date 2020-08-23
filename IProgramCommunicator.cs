using System.Threading.Tasks;

namespace Treasure.Checker
{
    public interface IProgramCommunicator
    {
        public Task<string> ReadLineAsync();
        public Task WriteLineAsync(string value);
    }
}