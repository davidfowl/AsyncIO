using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncIO
{
    internal static class TaskExtensions
    {
        private static readonly Task _emptyTask = MakeEmpty();

        public static Task Empty
        {
            get
            {
                return _emptyTask;
            }
        }

        public static Task Then(this Task task, Action successor)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return FromMethod(successor);
            }
            else if (task.Status == TaskStatus.Faulted)
            {
                return FromException(task.Exception);
            }

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    try
                    {
                        successor();
                        tcs.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }
            });

            return tcs.Task;
        }

        public static Task Then<T>(this Task<T> task, Action<T> successor)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return FromMethod(() => successor(task.Result));
            }
            else if (task.Status == TaskStatus.Faulted)
            {
                return FromException(task.Exception);
            }

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    try
                    {
                        successor(t.Result);
                        tcs.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }
            });

            return tcs.Task;
        }

        public static Task Then(this Task[] tasks, Action successor)
        {
            if (tasks.Length == 0)
            {
                return FromMethod(successor);
            }

            var tcs = new TaskCompletionSource<object>();
            Task.Factory.ContinueWhenAll(tasks, completedTasks =>
            {
                var faulted = completedTasks.FirstOrDefault(t => t.IsFaulted);
                if (faulted != null)
                {
                    tcs.SetException(faulted.Exception);
                    return;
                }
                var cancelled = completedTasks.FirstOrDefault(t => t.IsCanceled);
                if (cancelled != null)
                {
                    tcs.SetCanceled();
                    return;
                }

                successor();
                tcs.SetResult(null);
            });

            return tcs.Task;
        }

        public static Task Catch(this Task task, Action<Exception> handler)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Trace.TraceError(t.Exception.Message);
                    handler(t.Exception);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }


        private static Task MakeEmpty()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }

        private static Task FromException(Exception exception)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        private static Task FromMethod(Action successor)
        {
            var tcs = new TaskCompletionSource<object>();

            try
            {
                successor();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }
}
