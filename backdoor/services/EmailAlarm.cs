using Microsoft.Extensions.Configuration;

namespace backdoor.services;

public class EmailAlarm
{
    private readonly IConfiguration configuration;

    public EmailAlarm(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task SendAsyncEmail(string to, string subject, string body)
    {
        var fromEmail = configuration["Gmail:UserEmail"];
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("Missing configuration key: Gmail:UserEmail");
        }

        IMail mailService = new Gmail(to, subject, body, fromEmail, configuration);
        await mailService.SendMail();
    }
}