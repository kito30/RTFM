namespace backdoor.services;
using MimeKit;
using MailKit.Net.Smtp;

public class Mail : IMail
{
    public string to { get; private set; }
    public string subject { get; private set; }
    public string body { get; private set; }
    
    public string fromEmail { get; private set; }
    private readonly MimeMessage message = new();

    public Mail(string to, string subject, string body, string fromEmail)
    {
        this.to = to;
        this.subject = subject;
        this.body = body;
        this.fromEmail = fromEmail;
        MailInit();
    }

    private void MailInit()
    {
        message.From.Add(new MailboxAddress("System Alert", fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };
    }
    
    public void sendMail(string to, string subject, string body)
    {
        // Implementation for sending mail goes here
        // This is a placeholder and should be replaced with actual email sending logic
        Console.WriteLine($"Sending mail to: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
    }
}