using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Config
{
    [JsonObject(ItemRequired = Required.Always)]
    public class ConnectionStrings
    {
        public string DatabaseX { get; set; }
    }

    [JsonObject(ItemRequired = Required.Always)]
    public class Properties
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public string ServiceUrl { get; set; }
        public string Environment { get; set; }
        public string WebsiteUrl { get; set; }
    }

    [JsonObject(ItemRequired = Required.Always)]
    public class ServiceConfig
    {
        private static ServiceConfig _instance = null;
        private static readonly object _padlock = new object();
        private Properties _properties;

        private ServiceConfig() { }

        public static ServiceConfig Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ServiceConfig();
                    }
                    return _instance;
                }
            }
        }
        
        public Properties Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }
    }
}
