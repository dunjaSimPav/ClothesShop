using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace ClothesShop.Services
{
    public class EmailService : IEmailService
    {
        private const string Host = "localhost";
        private const int Port = 1025;
        private const string Sender = "admin@mailhog.local";

        public void SendEmail(string Address, string Subject, string Content)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(Sender);
                    mail.To.Add(Address);
                    mail.Subject = Subject;
                    mail.Body = Content;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(Host, Port))
                    {
                        smtp.Host = Host;
                        smtp.Port = Port;
                        smtp.UseDefaultCredentials = false;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        //smtp.Credentials = new NetworkCredential(Sender, Password);
                        smtp.EnableSsl = false;
                        smtp.Send(mail);
                    }
                }

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(Sender);
                    mail.To.Add(Sender);
                    mail.Subject = Subject;
                    mail.Body = Content;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(Host, Port))
                    {
                        smtp.Host = Host;
                        smtp.Port = Port;
                        smtp.UseDefaultCredentials = false;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        //smtp.Credentials = new NetworkCredential(Sender, Password);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (SmtpFailedRecipientException exception)
            {
                Debug.WriteLine(exception.StackTrace);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
