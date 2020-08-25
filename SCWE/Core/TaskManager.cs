using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SCWE
{
    public class TaskManager<T>
    {
        private Task<T>[] tasks;
        private Queue<int> emptySlots = new Queue<int>();

        public int RunningJobCount { get; private set; }

        public event EventHandler<T> OnTaskComplete;

        public TaskManager(int maxTaskCount)
        {
            if (maxTaskCount < 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            tasks = new Task<T>[maxTaskCount];
            for (int i = 0; i < maxTaskCount; i++)
            {
                emptySlots.Enqueue(i);
            }
        }

        public void QueueJob(Func<T> job)
        {
            if (emptySlots.Count == 0)
            {
                throw new Exception("Illegal state");
            }
            int index;
            index = emptySlots.Dequeue();
            tasks[index] = new Task<T>(job);
            tasks[index].Start();
            RunningJobCount += 1;
        }

        public void PollEvents()
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                var t = tasks[i];
                if (t != null && t.IsCompleted)
                {
                    t.Wait();
                    var res = t.Result;
                    OnTaskComplete(this, res);
                    emptySlots.Enqueue(i);
                    RunningJobCount -= 1;
                    tasks[i] = null;
                }
            }
        }

        public void WaitAll()
        {
            Task.WaitAll(tasks);
        }
    }
}
