namespace YouSummarize.Managers;

using System;
using System.IO;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

public class EmailManager(IConfiguration config)
{
    private const string SmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 465;

    public async Task SendEmailAsync(string subject, string bodyText, string bodyHtml, string[]? attachmentPaths = null)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(config["EMAIL_BACKUP_FROM"], config["EMAIL_BACKUP_FROM"]));
            email.To.Add(new MailboxAddress(config["EMAIL_BACKUP_DEST"], config["EMAIL_BACKUP_DEST"]));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = bodyText,
                HtmlBody = bodyHtml
            };

            // Ajout des pièces jointes si fournies
            if (attachmentPaths != null)
            {
                var existingFiles = attachmentPaths.Where(File.Exists);
                foreach (var filePath in existingFiles)
                {
                    await bodyBuilder.Attachments.AddAsync(filePath);
                }              
            }

            email.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(config["EMAIL_BACKUP_FROM"], config["SMTP_PASSWORD"]);
            await client.SendAsync(email);
            await client.DisconnectAsync(true);

            Console.WriteLine("✅ Email envoyé avec succès !");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de l'envoi de l'email : {ex.Message}");
        }
    }
}