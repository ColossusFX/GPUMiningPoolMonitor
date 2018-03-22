using MimeKit;
using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPUPoolMonitor
{
    public class EmailSender
    {
        private readonly string recipient;
        private readonly string emailAddress;
        private readonly string minerName;

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
        }

        // Loop collection of miners and check speeds
        public async Task SendEmailWorkerSpeedUpdate(Dictionary<string, decimal> dict)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("GPU Miner Monitor", emailAddress));
            message.To.Add(new MailboxAddress(recipient, emailAddress));
            message.Subject = "GPU Miner Monitor - Worker Speed";
            string content = null;

            foreach (var workers in dict)
            {
                content += "Worker Name \n" + workers.Key + "\n" + workers.Value + "\n" + "************************ \n\n";
            }

            message.Body = new TextPart("plain") { Text = content + DateTime.Now + "\n" };
            await EmailAuthAsync(message).ConfigureAwait(false);
        }

        public async Task EmailAuthAsync(MimeMessage message)
        {
            try
            {
                if (Program.UserData.MailServer != string.Empty || Program.UserData.MailPassword != string.Empty || Program.UserData.MailUserName != string.Empty)
                {
                    using (var client = new SmtpClient())
                    {
                        // For demo-purposes, accept all SSL certificates
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                        await client.ConnectAsync(Program.UserData.MailServer, 25, false).ConfigureAwait(false);

                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        client.AuthenticationMechanisms.Remove("XOAUTH2");

                        client.Authenticate(Program.UserData.MailUserName, Program.UserData.MailPassword);

                        client.Send(message);

                        Console.WriteLine("Email Sent");

                        client.Disconnect(true);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter mail settings in default.conf", "Warning", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
