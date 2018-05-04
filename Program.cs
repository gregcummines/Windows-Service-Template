using log4net;

namespace Template.Service
{
    public class Program
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(Program));
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            ConfigureService.Configure();
            ConfigureService.HostService();
        }
    }
}
