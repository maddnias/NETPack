using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NETPack.Core.Engine.Utils
{
    public interface IExceptionFormatter
    {
        string Format(Exception exception);
    }

    public class BugReporter
    {
        private const string ReportGenerator = "BugReporterC#";
        private const string Server = "http://bugster.dextrey.dy.fi/report.json";
        private readonly string _apiKey;
        private readonly IExceptionFormatter _exceptionFormatter;

        /// <summary>
        /// Gets called before report is sent to the server.
        /// Return false from the callback to prevent report from being sent.
        /// </summary>
        public Func<Exception, bool> UserAuthorizationCheck { get; set; }

        /// <summary>
        /// Raised when report delivery has either succeeded or failed
        /// </summary>
        public EventHandler<ReportCompletionEventArgs> ReportCompleted;

        public BugReporter(string apiKey, IExceptionFormatter exceptionFormatter = null)
        {
            _apiKey = apiKey;
            if (exceptionFormatter == null)
            {
                _exceptionFormatter = new DefaultExceptionFormatter();
            }
            else
            {
                _exceptionFormatter = exceptionFormatter;
            }

            UserAuthorizationCheck = x => true;
        }

        public bool ManualReport(string message, string @class = "Manual")
        {
            var builder = new StringBuilder();
            builder.AppendLine("Unhandled exception - caught at " + DateTime.Now.ToString(new CultureInfo("en-US")));
            builder.AppendLine();
            builder.AppendLine(message);

            var json = BuildRequestJson(message, @class);
            try
            {
                if (!DoRequest(json))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {

            var builder = new StringBuilder();
            builder.AppendLine("Unhandled exception - caught at " + DateTime.Now.ToString(new CultureInfo("en-US")));
            builder.AppendLine();
            builder.AppendLine(_exceptionFormatter.Format(e.ExceptionObject as Exception));

            if (IsDeliveryAuthorized(e.ExceptionObject as Exception))
            {
                DeliverReportToServer(builder.ToString(), "Unhandled exception");
            }
        }

        private bool IsDeliveryAuthorized(Exception exception)
        {
            /*TODO: more checks can be added here */
            return UserAuthorizationCheck(exception);
        }

        private void DeliverReportToServer(string message, string @class)
        {
            var json = BuildRequestJson(message, @class);
            try
            {
                if (!DoRequest(json))
                {
                    RaiseReportCompleted(false, false); // failure due to server result
                    return;
                }
            }
            catch
            {
                RaiseReportCompleted(false, true); // unexpected failure
                return;
            }
            RaiseReportCompleted(true, false); // success :P
        }

        private void RaiseReportCompleted(bool succeeded, bool failedThanksToException)
        {
            if (ReportCompleted != null)
            {
                ReportCompleted(this, new ReportCompletionEventArgs(succeeded, failedThanksToException));
            }
        }

        private bool DoRequest(string json)
        {
            var request = WebRequest.Create(Server) as HttpWebRequest;
            if (request == null)
                throw new Exception("");

            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "text/javascript";
            request.Timeout = 10 * 1000;

            using (var requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(json);
            }

            var response = request.GetResponse() as HttpWebResponse;
            string responseString = null;
            using (var responseReader = new StreamReader(response.GetResponseStream()))
            {
                responseString = responseReader.ReadToEnd();
            }

            if (responseString.Trim() == "OK")
            {
                return true;
            }
            return false;
        }

        private string BuildRequestJson(string message, string @class)
        {
            var package = new JsonRequestPackage();
            package.ApiKey = _apiKey;
            package.Generator = ReportGenerator;
            package.Format = "plain";
            package.Content = message;
            package.Class = @class;

            var jsonSerializer = new DataContractJsonSerializer(typeof(JsonRequestPackage));
            var stream = new MemoryStream();
            jsonSerializer.WriteObject(stream, package);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        [DataContract]
        private class JsonRequestPackage
        {
            [DataMember(Name = "api_key")]
            public string ApiKey;

            [DataMember(Name = "generator")]
            public string Generator;

            [DataMember(Name = "format")]
            public string Format;

            [DataMember(Name = "report_class")]
            public string Class;

            [DataMember(Name = "content")]
            public string Content;
        }
    }

    public class ReportCompletionEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates if they report was succesfully delivered or not
        /// </summary>
        public bool WasSuccesful { get; private set; }

        /// <summary>
        /// Set to true if delivery of the report failed due to exception during the delivery process
        /// False if failure of delivery was due to server refusing the request
        /// </summary>
        public bool FailedDueToException { get; private set; }

        public ReportCompletionEventArgs(bool succesful, bool failureDueToException)
        {
            WasSuccesful = succesful;
            FailedDueToException = failureDueToException;
        }
    }


    /// <summary>
    /// Provides simple exception formatting with Exception.ToString method
    /// </summary>
    internal class DefaultExceptionFormatter : IExceptionFormatter
    {
        public string Format(Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append(exception.ToString());
            return builder.ToString();
        }
    }
}
