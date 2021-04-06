using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlChecker.Web.Models.Abstract;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace UrlChecker.Web.Models.Concrete
{
    public class EmailService : ISenderService
    {
        private IConfiguration _configuration;

        public EmailService()
        {
        }

        public EmailService(IConfiguration iconfig)
        {
            _configuration = iconfig;
        }

        public void Send(string from, string to, string subject, string message)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            // send email
            using var smtp = new SmtpClient();

            // gmail
            //smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

            // hotmail
            //smtp.Connect("smtp.live.com", 587, SecureSocketOptions.StartTls);

            // office 365
            //smtp.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);

            string SmtpHost = "smtp.ethereal.email";
            int SmtpPort = 587;
            string SmtpUser = "alverta72@ethereal.email";
            string SmtpPass = "qF3GUfChxEhGcB6ZN6";

            smtp.Connect(SmtpHost, Convert.ToInt32(SmtpPort), SecureSocketOptions.StartTls);

            smtp.Authenticate(SmtpUser, SmtpPass);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
