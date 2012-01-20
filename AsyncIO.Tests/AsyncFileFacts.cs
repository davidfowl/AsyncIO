using System;
using System.IO;
using Xunit;

namespace AsyncIO.Tests
{
    public class AsyncFileFacts
    {
        public class CopyAsync
        {
            [Fact]
            public void CopiesFile()
            {
                File.WriteAllText("foo.txt", "Hello world");
                AsyncFile.CopyAsync("foo.txt", "foo2.txt").Wait();

                Assert.True(File.Exists("foo2.txt"));
                Assert.Equal("Hello world", File.ReadAllText("foo2.txt"));
            }

            [Fact]
            public void CopyNonExistentInputFileThrows()
            {
                Assert.Throws<FileNotFoundException>(() => AsyncFile.CopyAsync("bar.txt", "bar2.txt").Wait());
            }

            [Fact]
            public void BufferSizeLessThanOrEqualToZeroThrows()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => AsyncFile.CopyAsync("foo.txt", "bar2.txt", -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => AsyncFile.CopyAsync("foo.txt", "bar2.txt", 0));
            }

            [Fact]
            public void NullSourceThrows()
            {
                Assert.Throws<ArgumentNullException>(() => AsyncFile.CopyAsync(null, "bar2.txt", 10));
            }

            [Fact]
            public void NullDestThrows()
            {
                Assert.Throws<ArgumentNullException>(() => AsyncFile.CopyAsync("src.txt", null, 13));
            }
        }
    }
}
