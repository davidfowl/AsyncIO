using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncIO
{
    public static class AsyncDirectory
    {
        public static Task CopyAsync(string sourceDirectory, string destDirectory)
        {
            return CopyAsync(sourceDirectory, destDirectory, degreeOfParallelism: 5);
        }

        public static Task CopyAsync(string sourceDirectory, string destDirectory, int degreeOfParallelism)
        {
            var taskPool = new TaskPool(degreeOfParallelism);

            var source = new DirectoryInfo(sourceDirectory);
            var dest = new DirectoryInfo(destDirectory);

            // Add each copy task to the pool
            foreach (var task in GetCopyTasks(sourceDirectory, destDirectory, source, dest))
            {
                taskPool.Add(task);
            }

            // Drain the pool for all uncompleted tasks
            return taskPool.Drain();
        }

        private static IEnumerable<Task> GetCopyTasks(string sourcePath, string destPath, DirectoryInfo sourceDirectory, DirectoryInfo destDirectory)
        {
            if (!destDirectory.Exists)
            {
                destDirectory.Create();
            }

            var filesTasks = from file in sourceDirectory.GetFiles()
                             let targetPath = GetDestinationPath(sourcePath, destPath, file)
                             select AsyncFile.CopyAsync(file.FullName, targetPath);

            var directoryTasks = from directory in sourceDirectory.GetDirectories()
                                 let target = new DirectoryInfo(GetDestinationPath(sourcePath, destPath, directory))
                                 from subTask in GetCopyTasks(sourcePath, destPath, directory, target)
                                 select subTask;

            return filesTasks.Concat(directoryTasks);
        }

        private static string GetDestinationPath(string sourceRootPath, string destinationRootPath, FileSystemInfo info)
        {
            string sourcePath = info.FullName;
            sourcePath = sourcePath.Substring(sourceRootPath.Length)
                                   .Trim(Path.DirectorySeparatorChar);

            return Path.Combine(destinationRootPath, sourcePath);
        }
    }
}
