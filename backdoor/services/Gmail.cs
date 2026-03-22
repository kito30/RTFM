namespace backdoor.services;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

public class Gmail : IMail
{
    public string to { get; private set; }
    public string subject { get; private set; }
    public string body { get; private set; }
    
    public string fromEmail { get; private set; }
    private readonly MimeMessage message = new();
    private readonly SmtpClient client = new();
    private readonly IConfiguration _configuration;

    public Gmail(string to, string subject, string body, string fromEmail, IConfiguration configuration)
    {
        this.to = to;
        this.subject = subject;
        this.body = body;
        this.fromEmail = fromEmail;
        _configuration = configuration;
        MailInit();
    }
    private void MailInit()
    {
        message.From.Add(new MailboxAddress("System Alert", fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };
    }
    
    private async Task ClientInit()
    {
        var userEmail = _configuration["Gmail:UserEmail"];
        var appPassword = _configuration["Gmail:AppPassword"];

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new InvalidOperationException("Missing configuration key: Gmail:UserEmail");
        }

        if (string.IsNullOrWhiteSpace(appPassword))
        {
            throw new InvalidOperationException("Missing configuration key: Gmail:AppPassword");
        }

        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(userEmail, appPassword);
    }
    public async Task SendMail()
    {
        await ClientInit();
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}