using Market.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Market.Application.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["Email:SmtpHost"] ?? throw new ArgumentNullException(_smtpHost);
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? throw new ArgumentNullException(_smtpUsername);
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? throw new ArgumentNullException(_smtpPassword);
        _fromEmail = _configuration["Email:FromEmail"] ?? throw new ArgumentNullException(_fromEmail);
        _fromName = _configuration["Email:FromName"] ?? "Market API";
    }

    public async Task SendPasswordResetEmailAsync(string email, string firstName, string resetToken)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        var subject = "Password Reset Request";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>Hello {firstName},</p>
                <p>You have requested to reset your password. Click the link below to reset your password:</p>
                <p><a href='{resetUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you did not request this password reset, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Market API Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Welcome to Market API";
        var body = $@"
            <html>
            <body>
                <h2>Welcome to Market API!</h2>
                <p>Hello {firstName},</p>
                <p>Thank you for registering with Market API. Your account has been successfully created.</p>
                <p>You can now start using our services.</p>
                <br>
                <p>Best regards,<br>Market API Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string firstName)
    {
        var subject = "Password Changed Successfully";
        var body = $@"
            <html>
            <body>
                <h2>Password Changed</h2>
                <p>Hello {firstName},</p>
                <p>Your password has been successfully changed.</p>
                <p>If you did not make this change, please contact our support team immediately.</p>
                <br>
                <p>Best regards,<br>Market API Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        using var message = new MailMessage(_fromEmail, to)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.From = new MailAddress(_fromEmail, _fromName);

        await client.SendMailAsync(message);
    }
}