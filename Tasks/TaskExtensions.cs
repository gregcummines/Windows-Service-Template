using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Service.Tasks;

namespace Template.Service.Tasks
{
    public static class TaskExtensions
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(TaskExtensions));

        public static void SafeExecute(this ITask task, object extraData = null)
        {
            var taskRunner = task as ITask;

            // Read the interval off the class object attribute
            var intervalInSeconds = task.GetType()
                 .GetAttributeValue((TaskRepeatAttribute dna) => dna.IntervalInSeconds);

            // If the task is already executing, don't execute it again...
            if (!taskRunner.IsExecuting())
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var readableInterval = (new TimeSpan(0, 0, 0, intervalInSeconds)).ToReadableTime();
                Log($"Executing task (will execute every {readableInterval}): " + task.GetType().ToString());
                taskRunner.Execute(extraData);
                stopWatch.Stop();
                var timeToExecute = stopWatch.Elapsed.ToReadableTime();
                Log("Done Executing task: " + task.GetType().ToString() + ": Elapsed time: " + timeToExecute);
            }
            else
            {
                Log(task.GetType() + " was scheduled to execute but we are in the middle of executing that task already...");
            }
        }

        private static void Log(string entry)
        {
            Console.WriteLine(entry);
            Debug.WriteLine(entry);
            m_log.Info(entry);
        }
    }
}
