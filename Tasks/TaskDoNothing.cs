using System;
using System.Threading;

namespace Template.Service.Tasks
{
    [TaskRepeat(IntervalInSeconds = 360)]
    internal class TaskDoNothing : ITask
    {
        private bool _isExecuting = false;
        private CancellationTokenSource _taskCancellationToken;

        public void Execute(object extraData = null)
        {
            _isExecuting = true;
            Thread.Sleep(2000);
            _isExecuting = false;
        }

        public bool IsExecuting()
        {
            return _isExecuting;
        }

        public void SetCancellationToken(CancellationTokenSource token)
        {
            _taskCancellationToken = token;
        }
    }
}
