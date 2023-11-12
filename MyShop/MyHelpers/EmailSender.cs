using Newtonsoft.Json.Linq;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System.Diagnostics;

namespace MyShop.MyHelpers
{
    public class EmailSender
    {
        //OLD SendGrid CODEBLOCK
        //public static void SendEmail(string toEmail, string username, string subject, string message)
        //{
        //    string apiKey = "";                         //need an API key from a reliable SMTP email service. SendGrid is denying me.
        //    var client = new SendGridClient(apiKey);

        //    var from = new EmailAddress("", "");        //1st email address used to SEND the email notifications out to users. 2nd is the website name of the sender (this project)
        //    var to = new EmailAddress(toEmail, username);
        //    var plainTextContent = message;
        //    var htmlContent = "";

        //    var msg = MailHelper.CreateSingleEmail(
        //        from, to, subject, plainTextContent, htmlContent);

        //    var response = client.SendEmailAsync(msg);
        //}

        //Brevo CodeBlock
        public static void SendEmail(string toEmail, string username, string subject, string message)
        {
            string apiKey = "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";

            Configuration.Default.ApiKey["api-key"] = apiKey;

            var apiInstance = new TransactionalEmailsApi();
            string SenderName = "Martin Young";
            string SenderEmail = "martingyoung@hotmail.com";
            
            SendSmtpEmailSender emailSender = new SendSmtpEmailSender(SenderName, SenderEmail);
            SendSmtpEmailTo emailReceiver1 = new SendSmtpEmailTo(toEmail, username);

            List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
            To.Add(emailReceiver1);

            string HtmlContent = null;
            string TextContent = message;
            
            var sendSmtpEmail = new SendSmtpEmail(emailSender, To, null, null, HtmlContent, TextContent, subject);
            CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
            Console.WriteLine("Response: \n" + result.ToJson());
        }
    }
}
