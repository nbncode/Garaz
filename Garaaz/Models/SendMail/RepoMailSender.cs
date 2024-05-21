using System;
using System.Configuration;
using System.Net.Mail;

namespace Garaaz.Models
{
    public class RepoMailSender
    {
        public string MailSender(string to, string html, string subject)
        {
            string retVal;
            try
            {
                var msg = new MailMessage();
                var smtpClient = new SmtpClient();
                msg.From = new MailAddress(ConfigurationManager.AppSettings["MailTo"], ConfigurationManager.AppSettings["MailSenderDisplayName"]);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = html;
                msg.IsBodyHtml = true;
                smtpClient.Host = ConfigurationManager.AppSettings["MailHost"];
                smtpClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings["MailPort"]);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["MailSenderUserName"], ConfigurationManager.AppSettings["MailSenderPass"]);
                smtpClient.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                smtpClient.Send(msg);
                retVal = "true";

            }
            catch (Exception exc)
            {
                var baseExc = exc.GetBaseException();
                var stackTrace = $"Error occurred in sending mail. Stack Trace - {exc}";
                RepoUserLogs.SaveUserLogs(baseExc.Message, null, stackTrace);
                // lblMsg.Text = ex.Message;
                retVal = exc.Message;
            }

            return retVal;
        }

        public string MailSender(string to, string html, string subject, string filePath, string contentType)
        {
            string retVal;
            try
            {
                var msg = new MailMessage();
                var smtpClient = new SmtpClient();
                msg.From = new MailAddress(ConfigurationManager.AppSettings["MailSenderUserName"], ConfigurationManager.AppSettings["MailSenderDisplayName"]);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = html;
                msg.IsBodyHtml = true;
                msg.Attachments.Add(new Attachment(filePath, contentType));
                smtpClient.Host = ConfigurationManager.AppSettings["MailHost"];
                smtpClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings["MailPort"]);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["MailSenderUserName"], ConfigurationManager.AppSettings["MailSenderPass"]);
                smtpClient.EnableSsl = false;
                smtpClient.Send(msg);
                retVal = "true";

            }
            catch (Exception ex)
            {
                retVal = ex.Message;
            }

            return retVal;
        }

        public string MailHtml(string details, string username)
        {
            var str = "<table align='center' style='cursor: default; border-style: dashed; border-color: rgb(187, 187, 187); font-family: Verdana, Arial, Helvetica, sans-serif;'><tbody><tr><td data-mce-style='font-family: Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 14px; color: #666;' style='color: rgb(102, 102, 102); font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; font-size: 14px; margin: 8px; cursor: text; border-style: dashed; border-color: rgb(187, 187, 187);'><div data-mce-style='text-align: left; background-color: #fff; max-width: 500px; border-top: 10px solid #0088cc; border-bottom: 3px solid #0088cc;' style='max-width: 500px; border-top-width: 10px; border-top-style: solid; border-top-color: rgb(0, 136, 204); border-bottom-width: 3px; border-bottom-style: solid; border-bottom-color: rgb(0, 136, 204);'><div data-mce-style='padding: 10px 20px; color: #000; font-size: 20px; background-color: #efefef; border-bottom: 1px solid #ddd;' style='padding: 10px 20px; color: rgb(0, 0, 0); font-size: 20px; background-color: rgb(239, 239, 239); border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: rgb(221, 221, 221);'>" + ConfigurationManager.AppSettings["sitename"] + "</div><div data-mce-style='padding: 20px; background-color: #fff; line-height: 18px;' style='padding: 20px; line-height: 18px;'>";
            str += "<p data-mce-style='text-align: center;'>Dear " + username + "</p>";
            str += details;
            str += "<div></div></td></tr></tbody></table>";
            return str;
        }

        public string GetHtml(string username, string heading, string message)
        {
            //string SitePath = "http://garaaz.com";
            var sitePath = ConfigurationManager.AppSettings["WebUrl"];
            var logo = sitePath + "/assets/images/logo.png";
            var str = "<div style='font-family: source sans pro; margin: 0 auto' bgcolor='#ffffff'><table style='width: 100%' border='0' cellpadding='0' cellspacing='0'><tbody><tr><td style='padding: 0; margin: 0; background-color: #ffffff' align='center'><table style='max-width: 610px' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td style='text-align: left; vertical-align: top; font-size: 0; padding-top: 15px; padding-bottom: 15px; padding-left: 15px; background-color: #e2714c;text-align:center;'>";

            // str += "<a href='" + SiteUrl + "' target='_blank' style='display: block;color: white;font-size: 20px;font-weight: bold;text-decoration: none;font-family: cursive;'>" + SiteName + "</a></td></tr><tr><td style='padding: 0px 20px 20px 20px; margin: 0; background-color: #f0f4f5'><table style='width: 100%' border='0' cellpadding='0' cellspacing='0'><tbody><tr><td style='padding: 0; margin: 0; background-color: #f2f2f4' align='center'><table bgcolor='' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td><table bgcolor='' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td style='font-family: Arial; font-size: 13px; border-right: 2px solid #e3e3e3; border-left: 2px solid #e3e3e3'></td></tr></tbody></table></td></tr></tbody></table></td></tr>";
            str += "<a href='" + sitePath + "' target='_blank' style='display: block;color: white;font-size: 20px;font-weight: bold;text-decoration: none;font-family: cursive;'><img src=" + logo + " /></a></td></tr><tr><td style='padding: 0px 20px 20px 20px; margin: 0; background-color: #f0f4f5'><table style='width: 100%' border='0' cellpadding='0' cellspacing='0'><tbody><tr><td style='padding: 0; margin: 0; background-color: #f2f2f4' align='center'><table bgcolor='' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td><table bgcolor='' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td style='font-family: Arial; font-size: 13px; border-right: 2px solid #e3e3e3; border-left: 2px solid #e3e3e3'></td></tr></tbody></table></td></tr></tbody></table></td></tr>";

            str += "<tr><td style='padding: 0; margin: 0; background-color: #ffffff;' align='center'><table style='width: 100%' border='0' cellpadding='0' cellspacing='0'><tbody><tr><td style='padding: 0; margin: 0; font-family: Clan,'Helvetica Neue',Helvetica,Arial,sans-serif; font-size: 26px; color: #000000; line-height: 34px; padding: 0px 0px' align='center'></td></tr><tr><td style='padding: 0; margin: 0'></td></tr><tr><td style='padding: 20px 20px 2px 20px; margin: 0; border-right: 2px solid #e3e3e3; border-left: 2px solid #e3e3e3; border-bottom: 2px solid #e3e3e3' align='center'><table bgcolor='' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td><table bgcolor='' border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td style='font-family: Arial; font-size: 13px'><table style='width: 100%' border='0' cellpadding='0' cellspacing='0'><tbody><tr><td style='padding: 0px; margin: 0px; font-size: 19px; line-height: 30px; color: #999999; font-family: HelveticaNeue-Light,'Helvetica Neue Light','Helvetica Regular',Arial,sans-serif; padding: 0px 0px' align='left'>";
            if (!string.IsNullOrEmpty(username))
            {
                str += "<p style='font-size: 19px; line-height: 30px; color: #999999; font-family: HelveticaNeue-Light,'Helvetica Neue Light','Helvetica Regular',Arial,sans-serif'> Dear " + username + ", </p>";
            }

            str += "<p style='padding: 0px; margin: 0px; font-size: 19px; line-height: 30px; color: #999999; font-family: HelveticaNeue-Light,'Helvetica Neue Light','Helvetica Regular',Arial,sans-serif'>";

            str += message;

            str += "</td></tr><tr><td bgcolor='#ffffff' height='25'></td></tr></tbody></table><table border='0' cellpadding='0' cellspacing='0' width='100%' style='font-family: HelveticaNeue-Light,'Helvetica Neue Light','Helvetica Regular',Arial,sans-serif'><tbody><tr><td bgcolor='#ffffff' height=''></td></tr><tr><td bgcolor='#ffffff' height='' style='padding: 0px 0px 0px 0px'><hr></td></tr><tr><td bgcolor='#ffffff' height='5'></td></tr><tr><td><table border='0' cellpadding='0' cellspacing='0' width='100%'><tbody><tr><td width='50%' valign='top' align='center'>";
            // Remove social network table
            //"<table border='0' style='display:none;' cellpadding='0' cellspacing='0' width='100%' style='text-align: Center;'><tbody><tr><td style='padding-right: 20px'><span style='line-height: 20px; font-size: 13px'><a href='#' style='text-decoration: none' target='_blank'><img src='" + SitePath + "content/images/facebook-circle.png" + "' alt='Facebook' title='Facebook' border='0'></a></span><span style='line-height: 20px; font-size: 13px'><a href='" + SitePath + "' style='text-decoration: none' target='_blank'><img src='" + SitePath + "content/images/twitter-circle.png" + "' alt='Twitter' title='Twitter' border='0'></a></span><span style='line-height: 20px; font-size: 13px'><a href='#' style='text-decoration: none' target='_blank'><img src='" + SitePath + "content/images/google-circle.png" + "' alt='Google+' title='Google+' border='0'></a></span></td></tr></tbody></table>";
            str += "</td></tr></tbody></table></td></tr><tr><td bgcolor='#ffffff' height='1'><hr></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td bgcolor='#ffffff' height=''></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div>";
            return str;
        }
    }
}