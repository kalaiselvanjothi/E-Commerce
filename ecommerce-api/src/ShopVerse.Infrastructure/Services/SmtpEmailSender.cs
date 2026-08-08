using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var host        = _config["Smtp:Host"]        ?? "smtp.gmail.com";
        var port        = int.Parse(_config["Smtp:Port"] ?? "587");
        var username    = _config["Smtp:Username"]    ?? string.Empty;
        var password    = _config["Smtp:Password"]    ?? string.Empty;
        var senderEmail = _config["Smtp:SenderEmail"] ?? "no-reply@shopverse.com";
        var senderName  = _config["Smtp:SenderName"]  ?? "ShopVerse Security";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("[SMTP DEV MODE] Email to {ToEmail} skipped because SMTP credentials are not configured in appsettings.json.", toEmail);
            return;
        }

        try
        {
            using var client = new SmtpClient();
            client.Timeout = 10000; // 10s timeout

            var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(host, port, socketOptions);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Successfully sent email '{Subject}' to {ToEmail} via {Host}", subject, toEmail, host);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP host {Host}: {Message}", toEmail, host, ex.Message);
            // Non-blocking for forgot password execution flow
        }
    }
}
