using System.Threading;

namespace Template.Service.Tasks
{
    public interface ITask
    {
        bool IsExecuting();
        void Execute(object extraData = null);
        void SetCancellationToken(CancellationTokenSource token);
    }
}
