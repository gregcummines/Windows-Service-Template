using System.ServiceModel;
using System.ServiceModel.Web;

namespace Template.Service
{
    [ServiceContract]
    interface ITemplateRESTService
    {
		/// <summary>
		/// This method asks a task to do something.
		/// </summary>
		/// <returns>void</returns>
		[OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "task-do-something",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void TaskDoSomething();

        /// <summary>
        /// This method returns true if the task is busy doing something
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "is-task-doing-something",
            ResponseFormat = WebMessageFormat.Json)]
        bool IsTaskDoingSomething();

        [OperationContract]
        [WebGet(
            UriTemplate = "is-alive",
            ResponseFormat = WebMessageFormat.Json)]
        bool Ping();
    }
}
