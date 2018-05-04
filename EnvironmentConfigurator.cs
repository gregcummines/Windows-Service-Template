using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using log4net;
using log4net.Config;
using log4net.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Template.Config;

namespace Template.Service
{
    public static class EnvironmentConfigurator
	{
        private static readonly ILog m_log = LogManager.GetLogger(typeof(EnvironmentConfigurator));

        public static string GetServiceName()
        {
            return "Template." + ServiceConfig.Instance.Properties.Environment + ".Service";
        }

        public static void ConfigureServiceEnvironment()
		{
			// Configure logging to use app config file (which specifies the log file)
			XmlConfigurator.Configure();

			var applicationPath = AppDomain.CurrentDomain.BaseDirectory;
            m_log.Info($"Application path is {applicationPath}");
            var applicationConfigurationPath = Path.Combine(applicationPath, "Config");

			// Initialize our environment specific configuration settings from config.json
		    var configFileNames = new List<string>();

            // Start with the main configuration file, then add the local,
            // and then finally add the user specific (These files will only get used
            // if they are found in below code)
            
            configFileNames.Add(@"config.json");

// Only add in other config files in DEBUG mode
#if (DEBUG)
            var userIdentity = Environment.UserName;
            configFileNames.Add(@"config.local.json");
            configFileNames.Add($@"config.{userIdentity}.json");
#endif 
            var json = string.Empty;

            var jsonConfigs = new List<string>();
            foreach (var file in configFileNames)
            {
                var filePath = Path.Combine(applicationConfigurationPath, file);
                if (File.Exists(filePath))
                {
                    jsonConfigs.Add(File.ReadAllText(filePath));
                }
            }

            m_log.Info($"{jsonConfigs.Count} configuration files found...");
            if (jsonConfigs.Count > 0)
            {
                JObject j1 = JObject.Parse(jsonConfigs[0]);
                for (int i = 0; i < (jsonConfigs.Count - 1); i++)
                {
                    // Merge the next
                    var j2 = JObject.Parse(jsonConfigs[i + 1]);
                    j1.Merge(j2, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge
                    });
                }
                m_log.Info("Initializing environment using config files...");
                ServiceConfig.Instance.Properties = JsonConvert.DeserializeObject<Properties>(j1.ToString());
            }
            else
            {
                throw new Exception($"There were no configuration files found at: {applicationConfigurationPath}");
            }

            m_log.Info("Changing log4net file name to match computer name and environment");
            // Now that the environment is loaded, change the log4net file name
            ChangeLogFileName(Environment.MachineName, ServiceConfig.Instance.Properties.Environment);

            //m_log.Info("Initializing the exception handler..");
            // Initialize the exception handler
            //ExceptionReporterFactory.InitializeExceptionHandler(applicationPath, ServiceConfig.Instance.Properties.ConnectionStrings.Template, "Template.Service");
		}

		/// <summary>
		/// Changes the name of the log4net file name at runtime when service is first started
		/// </summary>
		/// <param name="name"></param>
		private static void ChangeLogFileName(string computerName, string environmentName)
		{
			var rootRep = LogManager.GetRepository(Assembly.GetCallingAssembly());

			XmlElement section = ConfigurationManager.GetSection("log4net") as XmlElement;

			XPathNavigator navigator = section.CreateNavigator();
			XPathNodeIterator nodes = navigator.Select("appender/param[@name='File']");
			foreach (XPathNavigator appender in nodes)
			{
				appender.MoveToAttribute("value", string.Empty);
				var value = appender.Value;
				value = value.Replace("{0}", computerName + "." + environmentName);
				appender.SetValue(value);
			}

			var xmlCon = rootRep as IXmlRepositoryConfigurator;
			xmlCon?.Configure(section);
		}
	}
}