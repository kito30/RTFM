namespace backdoor.services;
using MimeKit;
using MailKit.Net.Smtp;

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
    
    private async void ClientInit()
    {
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_configuration["Gmail:UserEmail"], _configuration["Gmail:AppPassword"]);
    }
}