using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Template.Service.Tasks;


namespace Template.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class TemplateRESTService : ITemplateRESTService
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(TemplateRESTService));
        private static CancellationTokenSource _windowsServiceStopCancellationToken;
        private IEnumerable<ITask> tasks;
        
        public TemplateRESTService(CancellationTokenSource windowsServiceStopCancellationToken)
        {
            Log("ENTER TemplateRESTService ctor...");
            _windowsServiceStopCancellationToken = windowsServiceStopCancellationToken;
        }

        public void SetPeriodicTasks(IEnumerable<ITask> tasks)
        {
            this.tasks = tasks;
        }
                
        private void Log(string entry)
        {
            Debug.WriteLine(entry);
            m_log.Info(entry);
        }


        public void TaskDoSomething()
        {
            foreach (var task in tasks)
            {
                if (task.GetType().ToString() == "Template.Service.Tasks.TaskDoSomething")
                {
                    if (!task.IsExecuting())
                    {
                        Task.Run(() => {
                            try
                            {
                                task.SafeExecute();
                            }
                            catch (Exception e)
                            {
                                ExceptionReporter.ReportException(e);
                            }
                        });
                    }
                }
            }
        }

        public bool IsTaskDoingSomething()
        {
            var result = false;
            foreach (var task in tasks)
            {
                if (task.GetType().ToString() == "Template.Service.Tasks.TaskDoSomething")
                {
                    if (task.IsExecuting())
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public bool Ping()
	    {
		    return true;
	    }
    }
}
