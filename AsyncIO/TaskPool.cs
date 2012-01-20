using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO
{
    /// <summary>
    /// Controls how many concurrent tasks can run
    /// </summary>
    internal class TaskPool
    {
        private readonly ConcurrentDictionary<Task, bool> _tasks = new ConcurrentDictionary<Task, bool>();
        private readonly int _poolSize;
        private readonly TaskCompletionSource<object> _tcs;
        private int _resultSet;

        public TaskPool(int poolSize)
        {
            if (poolSize < 1)
            {
                throw new ArgumentOutOfRangeException("poolSize");
            }

            _poolSize = poolSize;
            _tcs = new TaskCompletionSource<object>();
        }

        public void Add(Task task)
        {
            if (_tasks.Count >= _poolSize)
            {
                var tasks = _tasks.Keys.ToArray();

                try
                {
                    Task.WaitAny(tasks);
                }
                catch (Exception ex)
                {
                    Fail(ex);
                    return;
                }
            }

            _tasks.TryAdd(task, true);

            task.Then(() =>
            {
                RemoveTask(task);

            }).Catch(ex =>
            {
                RemoveTask(task);
                Fail(ex);
            });
        }

        public Task Drain()
        {
            _tasks.Keys.ToArray().Then(() =>
            {
                // Mark the operation as complete
                _tcs.SetResult(null);

                // Clear the list
                _tasks.Clear();

            })
            .Catch(Fail);

            return _tcs.Task;
        }

        private void Fail(Exception exception)
        {
            if (Interlocked.Exchange(ref _resultSet, 1) == 0)
            {
                _tcs.SetException(exception);
            }
        }

        private void RemoveTask(Task task)
        {
            bool removed;
            _tasks.TryRemove(task, out removed);
        }
    }
}
