using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Config;
using Topshelf;

namespace Template.Service
{
    internal static class ConfigureService
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ConfigureService));

        internal static void Configure()
        {
            try
            {
                EnvironmentConfigurator.ConfigureServiceEnvironment();
                m_log.Info("*******************************************************");
                m_log.Info("Template Main Program starting...");
                m_log.Info($"{ServiceConfig.Instance.Properties.Environment} environment successfully configured...");
            }
            catch (Exception e)
            {
                m_log.Fatal(e.ToString());
                Console.WriteLine("Failure configuring environment: " + Environment.NewLine + e.ToString());
                return;
            }
        }

        internal static void HostService()
        {
            var environment = ServiceConfig.Instance.Properties.Environment;

            // See http://topshelf-project.com/ for more details
            // Topshelf is a framework for hosting services written using the .NET framework. 
            // The creation of services is simplified, allowing developers to create a simple 
            // console application that can be installed as a service using Topshelf. 
            // The reason for this is simple: It is far easier to debug a console application 
            // than a service. And once the application is tested and ready for production, 
            // Topshelf makes it easy to install the application as a service.
            HostFactory.Run(x =>
            {
                m_log.Info("ENTER TopShelf -- HostFactory.Run");
                x.Service<IService>(s =>
                {
                    s.ConstructUsing(name => new TemplateService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsPrompt();
                x.SetDescription($"Template {environment} Service");
                
                x.SetDisplayName($"Template.{environment}.Service");
                x.SetServiceName($"Template.{environment}.Service");

                m_log.Info("EXIT TopShelf -- HostFactory.Run");
            });
        }
    }
}
