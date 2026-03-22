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
        var fromEmail = configuration["Gmail:UserEmail"] ?? configuration["Email:UserEmail"] ?? string.Empty;
        IMail mailService = new Gmail(to, subject, body, fromEmail, configuration);
        await mailService.SendMail();
    }
}