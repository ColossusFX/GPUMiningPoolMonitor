using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GPUPoolMonitor
{
    public class EmailSender
    {
        private readonly string recipient;
        private readonly string emailAddress;
        private readonly string minerName;
        //private double speed;

        public EmailSender(string name, string email)
        {
            recipient = name;
            emailAddress = email;
        }

        public EmailSender(string name, string email, string worker)
        {
            recipient = name;
            emailAddress = email;
            minerName = worker;
            //speed = workerSpeed;
        }

        public void SendEmailWorkerMinerCount(string workername, DateTime date)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Miner Monitor", "contact@iamaflip.co.uk"));
            message.To.Add(new MailboxAddress(recipient, emailAddress));
            message.Subject = "Miner Monitor - Worker Down Alert";

            message.Body = new TextPart("plain") { Text = DateTime.Now + "\n" + "Worker Offline = " + workername + "\n" + "Last Seen = " + date };

            EmailAuth(message);
        }

        // Loop collection of miners and check speeds
        public void SendEmailWorkerSpeedUpdate(Dictionary<string, decimal> dict)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Miner Monitor", "contact@iamaflip.co.uk"));
            message.To.Add(new MailboxAddress(recipient, emailAddress));
            message.Subject = "Miner Monitor - Worker Speed";
            string content = null;

            foreach (var workers in dict)
            {
                //Console.WriteLine("Worker Speed \n" + workers.Key + "\n" + workers.Value + "\n");

                content += "Worker Name \n" + workers.Key + "\n" + workers.Value + "\n" + "************************ \n\n";
            }

            message.Body = new TextPart("plain") { Text = content + DateTime.Now + "\n" };
            EmailAuth(message);
        }

        public void EmailAuth(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                //client.Connect("p3plcpnl0656.prod.phx3.secureserver.net", 993, true);
                client.Connect("mail.iamaflip.co.uk", 25, false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate("monitor@coincatcher.uk", "4HGifTJYAOAJV3uIWlfq");

                client.Send(message);

                Console.WriteLine("Email Sent");

                client.Disconnect(true);
            }
        }
    }
}
