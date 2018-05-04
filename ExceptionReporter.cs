using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Template.Service
{
    internal class ExceptionReporter
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ExceptionReporter));

        public static async void ReportException(Exception exception)
        {
            try
            {
                m_log.Error("ExceptionReport.ReportException: " + exception.ToString());
                //await ExceptionReporterFactory.ReportExceptionAsync(
                //    false,
                //    exception,
                //    "Template.Service",
                //    ServiceConfig.Instance.Properties.Environment,
                //    Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (Exception)
            {
                m_log.Fatal("Exception reporting exception! This is bad...");
            }
        }
    }
}
