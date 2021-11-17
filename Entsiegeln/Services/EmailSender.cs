using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Entsiegeln.Services
{
    public class EmailSender:IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("mail@berlin-entsiegeln.de");
            mail.To.Add(email);
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = htmlMessage;

            SmtpClient smtp = new SmtpClient("mail.berlin-entsiegeln.de");
            NetworkCredential Credentials = new NetworkCredential("mail@berlin-entsiegeln.de", "NFsicher.21");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = Credentials;
            smtp.Port = 25;    //alternative port number is 8889
            smtp.EnableSsl = false;
            smtp.Send(mail);

            return Task.CompletedTask;
        }
    }
}
