using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using Template.Config;
using Template.Service.Tasks;


namespace Template.Service
{
    public class TemplateService : IService
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(TemplateService));
        private CancellationTokenSource _windowsServiceStopCancellationTokenSource = new CancellationTokenSource();
        private List<Task> _tasks = new List<Task>();
        private ServiceHost wcfServiceHost;

        public TemplateService()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

#if DEBUG
        public void DebugThisService()
        {
            Startup();
        }
#endif

        private void Startup()
        {
            Log("");
            Log("**************************************************************");
            Log("Template.Service version: " + GetVersion() + " has been started.");
            Log("ENTER Startup...");
            Log("App.config info:");

            try
            {
                var otherComputerName = string.Empty;
                //if (IsAnotherInstanceRunning(out otherComputerName))
                //{
                //    var thisComputerName = Environment.MachineName;
                //    throw new Exception($"Attempting to start instance on {thisComputerName}, but another instance of this service is already running on {otherComputerName}!");
                //}
                //else
                //{
                //    RegisterServiceInstance();
                    Log("Startup creating instance of TemplateRESTService...");

                    var TemplateRESTService = new TemplateRESTService(_windowsServiceStopCancellationTokenSource);
                    
                    var baseAddress = new Uri(ServiceConfig.Instance.Properties.ServiceUrl);
                    wcfServiceHost = new WebServiceHost(TemplateRESTService, baseAddress);
                    Log("Calling TemplateRESTService.Open()...");
                    wcfServiceHost.Open();

                    var address = wcfServiceHost.BaseAddresses.First();
                    Log($"TemplateRESTService base address is {wcfServiceHost.BaseAddresses.First()}");

                    // Schedule tasks that run periodically
                    var tasks = SchedulePeriodicRunningTasks(_windowsServiceStopCancellationTokenSource);
                    TemplateRESTService.SetPeriodicTasks(tasks);
                //}
            }
            catch (Exception e)
            {
                Log("Exception in Startup: " + e.ToString());
                ExceptionReporter.ReportException(e);
            }

            Log("EXIT Startup...");
        }

        public void Start()
        {
            Log("ENTER Start...");
            try
            {
                Startup();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                ExceptionReporter.ReportException(ex);
            }

            Log("EXIT Start...");
        }

        public void Stop()
        {
            Log("ENTER Stop...");

            if (wcfServiceHost != null && wcfServiceHost.State == CommunicationState.Opened)
            {
                wcfServiceHost.Close();
            }

            // Issue the cancel to our background thread
            _windowsServiceStopCancellationTokenSource.Cancel();

            Log("Attempting to stop Template.Service, export task cancel has been issued...");

            // Wait for our background task to finish or cancel
            try
            {
                foreach (var task in _tasks)
                {
                    if (task != null)
                    {
                        Log("Waiting for task to end...");
                        task.Wait();
                    }
                }
            }
            catch (AggregateException e)
            {
                Log("AggregateException thrown with the following inner exceptions:");
                // Display information about each exception.  
                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                        Log("Task was cancelled..." + e.ToString());
                    else
                        Log("Task threw exception in AggregateException..." + e.ToString());
                }
            }
            catch (Exception e)
            {
                Log("Task threw exception..." + e.ToString());
            }
            finally
            {
                //UnregisterServiceInstance();
            }
            Log("EXIT Stop...");
        }

        /// <summary>
        /// Method to get the file version from the executing assembly
        /// </summary>
        /// <returns></returns>
        private string GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            return version;
        }

        //private bool IsAnotherInstanceRunning(out string otherComputerName)
        //{
        //    using (var db = new PetaPoco.Database(ServiceConfig.Instance.Properties.ConnectionStrings.Template))
        //    {
        //        var sql = @"SELECT [Value] FROM [dbo].[ApplicationState]
        //                WHERE [Property] = @0";

        //        otherComputerName = db.ExecuteScalar<string>(sql, "Template.Service.Instance");

        //        // If string is not empty or null another computer is already running this service!
        //        return !string.IsNullOrEmpty(otherComputerName?.Trim()) && otherComputerName != Environment.MachineName;
        //    }
        //}

        //private void RegisterServiceInstance()
        //{
        //    using (var db = new PetaPoco.Database(ServiceConfig.Instance.Properties.ConnectionStrings.Template))
        //    {
        //        // register this instance as running on this computer
        //        var sql = $@"UPDATE [dbo].[ApplicationState]
        //                SET [Value] = '{Environment.MachineName}' WHERE [Property] = 'Template.Service.Instance'";

        //        db.Execute(sql);
        //    }
        //}

        //private void UnregisterServiceInstance()
        //{
        //    using (var db = new PetaPoco.Database(ServiceConfig.Instance.Properties.ConnectionStrings.Template))
        //    {
        //        var sql = $@"UPDATE [dbo].[ApplicationState]
        //                SET [Value] = '' WHERE [Property] = 'Template.Service.Instance'";
        //        db.Execute(sql);
        //    }
        //}

        private IEnumerable<ITask> SchedulePeriodicRunningTasks(CancellationTokenSource windowsServiceStopCancellationTokenSource)
        {
            Log("ENTER StartPeriodicRunningTasks...");

            // Dynamically find all tasks and execute them in the right order
            var tasks = (from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.GetInterfaces().Contains(typeof(ITask))
                                 && t.GetConstructor(Type.EmptyTypes) != null
                        select Activator.CreateInstance(t) as ITask).ToList();

            // Allow each class that implements ITaskRunner to handle the exception on its own task
            foreach (var task in tasks)
            {
                try
                {
                    Log("Scheduling the " + task.GetType() + " task.");

                    // Read the interval off the class object attribute
                    var intervalInSeconds = task.GetType()
                         .GetAttributeValue((TaskRepeatAttribute dna) => dna.IntervalInSeconds);

                    if (intervalInSeconds == 0)
                        throw new Exception("Class either does not have the TaskRepeatAttribute or the interval is set incorrectly.");

                    // Set a cancellation token on each task, so that each task can 
                    // check to see if they need to stop what they are doing and respond to 
                    // a service shutdown or similar. 
                    task.SetCancellationToken(windowsServiceStopCancellationTokenSource);

                    // Ask the PeriodicTaskFactory to execute our action every x seconds. 
                    // The class implementing the ITaskRunner gets to decide how often it wants to run.
                    var perdiodicTask = PeriodicTaskFactory.Start(() =>
                    {
                        try
                        {
                            task.SafeExecute();
                        }
                        catch (Exception ex)
                        {
                            Log("Unhandled exception trying to execute the task: " + task.GetType().ToString() + ". " + ex.Message);
                            ExceptionReporter.ReportException(ex);
                        }
                    },
                    intervalInSeconds: intervalInSeconds,
                    cancelToken: _windowsServiceStopCancellationTokenSource.Token); // fire every x milliseconds...

                    perdiodicTask.ContinueWith(
                        c =>
                        {
                            AggregateException exception = c.Exception;

                            Log("Unhandled exception in task, details follow...");
                            foreach (var v in exception.InnerExceptions)
                            {
                                if (v is TaskCanceledException)
                                    Log("Task was cancelled..." + v.ToString());
                                else
                                {
                                    Log("Task threw exception..." + v.ToString());
                                    ExceptionReporter.ReportException(v);
                                }
                            }
                        },
                        TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously
                    ).ContinueWith(
                        c =>
                        {
                            Log("Task has been completed");
                        },
                        TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously
                    );

                    _tasks.Add(perdiodicTask);
                }
                catch (Exception ex)
                {
                    Log("Unhandled exception trying to start the task: " + task.GetType().ToString() + ". " + ex.Message);
                    ExceptionReporter.ReportException(ex);
                }
            }
            return tasks;
        }

        private void Log(string entry)
        {
            Console.WriteLine(entry);
            Debug.WriteLine(entry);
            m_log.Info(entry);
        }
    }
}
