namespace backdoor.interfaces;

public interface IMail
{
    public string to { get; }
    public string subject { get; }
    public string body { get; }
    public Task SendMail();

}