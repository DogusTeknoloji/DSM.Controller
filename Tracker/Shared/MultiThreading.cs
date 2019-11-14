using DSM.Core.Ops;
using DSM.Core.Ops.ConsoleTheming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DSM.Controller.Tracker.Shared
{
    public static class MultiThreading
    {
        private const int TASK_LIMIT = 32;

        public static Queue<Task> TaskQueue = new Queue<Task>();
        public static int ActiveTaskCounter = 0;

        public static void Run(this IList<Task> tasks)
        {
            XConsole.SetDefaultColorSet(ConsoleColorSetGreen.Instance);

            TaskQueue = TaskQueue ?? new Queue<Task>();
            TaskQueue = TaskQueue.Reload(tasks);

            int taskQ = TaskQueue.Count;
            while (TaskQueue.Count > 0)
            {
                if (ActiveTaskCounter < TASK_LIMIT && TaskQueue.Count > 0)
                {
                    Task currentOperation = TaskQueue.Dequeue();
                    XConsole.Progress(string.Empty, taskQ - TaskQueue.Count - ActiveTaskCounter, taskQ);
                    //XConsole.WriteLine("Active Task(s): " + ActiveTaskCounter + ", TaskQueue: " + TaskQueue.Count + ", Current Ops:" + currentOperation.Id);
                    currentOperation.Start();
                    ActiveTaskCounter++;

                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }
            }

            Task.WaitAll(tasks.ToArray());

            //while (ActiveTaskCounter > 0)
            //{
            //    XConsole.Progress("Task Progress", taskQ - TaskQueue.Count - ActiveTaskCounter, taskQ);
            //    //XConsole.WriteLine("Remaining Task(s): " + ActiveTaskCounter + ", TaskQueue: Completed");
            //    Thread.Sleep(TimeSpan.FromSeconds(1));
            //}
        }

    }
}
