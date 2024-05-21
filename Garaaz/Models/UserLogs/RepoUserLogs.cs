using System;
using System.Web;

namespace Garaaz.Models
{
    public class RepoUserLogs
    {
        #region Save User Logs

        public static bool SaveUserLogs(string Status, string UserId, string Details)
        {
            garaazEntities db = new garaazEntities();
            Log log = new Log();
            log.CreatedDate = DateTime.Now;
            log.Status = Status;
            log.UserId = UserId;
            log.Details = Details;
            db.Logs.Add(log);
            return db.SaveChanges() > 0;
        }

        #endregion

        #region Exception Handling
        public static ResultModel SendExceptionMailFromController(string controller, string action, string Message, string StackTrack)
        {
            SaveUserLogs(Message, null, StackTrack);

            //SendMail("controller=" + controller + "<br/>action=" + action + "<br/>" + "Message=" + Message + "<br/>" + "StackTrack=" + StackTrack);
            return new ResultModel
            {
                ResultFlag = 0,
                Message = "Too many users in system please try again later."
            };
        }

        public static ResultModel SendExceptionMailFromController(string controller, string action, Exception exc)
        {
            // Log exception and inner exception
            string errorMessage = exc.Message, stackTrace = exc.StackTrace;
            var innerExc = exc.InnerException;
            while (innerExc != null)
            {
                errorMessage = innerExc.Message;
                stackTrace = innerExc.StackTrace;
                innerExc = innerExc.InnerException;
            }

            SaveUserLogs(errorMessage, null, stackTrace);

            SendMail("controller=" + controller + "<br/>action=" + action + "<br/>" + "Message=" + errorMessage + "<br/>" + "StackTrack=" + stackTrace);
            return new ResultModel
            {
                ResultFlag = 0,
                Message = errorMessage
            };
        }

        public static void SendExceptionMail(string Heading, string Message, string StackTrack)
        {
            SendMail("Heading=" + Heading + "<br/>Message =" + Message + "<br/>" + "StackTrack=" + StackTrack);
        }

        public static void SendMail(string Details)
        {
            RepoMailSender sendmail = new RepoMailSender();
            var dev = System.Configuration.ConfigurationManager.AppSettings["DeveloperMailId"];
            sendmail.MailSender(dev, Details, "Garaaz Exception");
        }

        /// <summary>
        /// Log exception into Logs in database.
        /// </summary>
        /// <param name="exc">The instance of Exception.</param>
        public static void LogException(Exception exc)
        {
            var request = HttpContext.Current?.Request;

            string errorMessage = exc.Message, stackTrace = exc.StackTrace;
            var innerExc = exc.InnerException;
            while (innerExc != null)
            {
                errorMessage = innerExc.Message;
                stackTrace = innerExc.StackTrace;
                innerExc = innerExc.InnerException;
            }

            if (request != null)
            {
                var controller = request.RequestContext.RouteData.GetRequiredString("controller");
                var action = request.RequestContext.RouteData.GetRequiredString("action");

                var details = $"Error occurred in action method '{action}' within controller '{controller}'. Stack Trace - {stackTrace}";
                SaveUserLogs(errorMessage, null, details);
            }
            else
            {
                SaveUserLogs(errorMessage, null, stackTrace);
            }
        }
        #endregion
    }
}