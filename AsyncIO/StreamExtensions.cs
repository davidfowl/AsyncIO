using System.IO;
using System.Threading.Tasks;

namespace AsyncIO
{
    internal static class StreamExtensions
    {
        public static Task<int> ReadAsync(this Stream stream, byte[] buffer)
        {
            return Task.Factory.FromAsync((cb, state) => stream.BeginRead(buffer, 0, buffer.Length, cb, state), ar => stream.EndRead(ar), null);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            return Task.Factory.FromAsync((cb, state) => stream.BeginWrite(buffer, offset, count, cb, state), ar => stream.EndWrite(ar), null);
        }
    }
}
