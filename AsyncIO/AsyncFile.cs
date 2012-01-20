using System;
using System.IO;
using System.Threading.Tasks;

namespace AsyncIO
{
    internal static class AsyncFile
    {        
        private static void CopyAsync(byte[] buffer, FileStream inputStream, Stream outputStream, TaskCompletionSource<object> tcs)
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
