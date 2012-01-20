using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AsyncIO.Tests
{
    public class TaskPoolFacts
    {
        public class Add
        {
            [Fact]
            public void FaultedTaskExceptionIsSet()
            {
                var taskPool = new TaskPool(1);

                taskPool.Add(Task.Factory.StartNew(() => { throw new Exception("This is a test"); }));

                var ex = Assert.Throws<AggregateException>(() => taskPool.Drain().Wait());
                Assert.Equal("This is a test", ex.GetBaseException().Message);
            }

            [Fact]
            public void AddingMoreThanPoolSizeWaits()
            {
                var taskPool = new TaskPool(1);

                long value = 0;
                taskPool.Add(Task.Factory.StartNew(() => { Thread.Sleep(500); Interlocked.Increment(ref value); }));
                taskPool.Add(Task.Factory.StartNew(() => { Thread.Sleep(500); Interlocked.Increment(ref value); }));
                taskPool.Add(Task.Factory.StartNew(() => { Thread.Sleep(500); Interlocked.Increment(ref value); }));

                taskPool.Drain().Wait();
                Assert.Equal(3, value);
            }

            [Fact]
            public void MultipleTasksBiggerThanPoolSizeOneThrows()
            {
                var taskPool = new TaskPool(1);

                long value = 0;
                taskPool.Add(Task.Factory.StartNew(() => { Thread.Sleep(1000); Interlocked.Increment(ref value); }));
                taskPool.Add(Task.Factory.StartNew(() => { Thread.Sleep(100); throw new InvalidOperationException("failed!"); }));
                taskPool.Add(Task.Factory.StartNew(() => { Thread.Sleep(1000); Interlocked.Increment(ref value); }));

                var ex = Assert.Throws<AggregateException>(() => taskPool.Drain().Wait());
                Assert.Equal("failed!", ex.GetBaseException().Message);
            }
        }
    }
}
