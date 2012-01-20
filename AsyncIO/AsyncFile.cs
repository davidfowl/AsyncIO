using System;
using System.IO;
using System.Threading.Tasks;

namespace AsyncIO
{
    public static class AsyncFile
    {
        public static Task CopyAsync(string sourceFileName, string destFileName)
        {
            return CopyAsync(sourceFileName, destFileName, 1024);
        }

        public static Task CopyAsync(string sourceFileName, string destFileName, int bufferSize)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }

            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            var inputStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
            var outputStream = new FileStream(destFileName, FileMode.Create, FileAccess.Write, FileShare.Write, bufferSize, useAsync: false);
            var tcs = new TaskCompletionSource<object>();

            var buffer = new byte[bufferSize];

            CopyAsync(buffer, inputStream, outputStream, tcs);

            return tcs.Task;
        }

        private static void CopyAsync(byte[] buffer, FileStream inputStream, FileStream outputStream, TaskCompletionSource<object> tcs)
        {
            inputStream.ReadAsync(buffer).Then(read =>
            {
                if (read > 0)
                {
                    outputStream.WriteAsync(buffer, 0, read)
                                .Then(() => CopyAsync(buffer, inputStream, outputStream, tcs))
                                .Catch(ex =>
                                {
                                    inputStream.Close();
                                    outputStream.Close();
                                    tcs.SetException(ex);
                                });
                }
                else
                {
                    inputStream.Close();
                    outputStream.Close();

                    tcs.SetResult(null);
                }
            })
            .Catch(ex =>
            {
                inputStream.Close();
                outputStream.Close();

                tcs.SetException(ex);
            });
        }
    }
}
