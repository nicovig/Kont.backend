namespace Kont.backend.Models;

public class AppSettings
{
    private string baseUrl = null!;

    public string BaseUrl { get => baseUrl; set => baseUrl = value.TrimEnd('/'); }

    private string frontUrl = null!;

    public string FrontUrl { get => frontUrl; set => frontUrl = value.TrimEnd('/'); }

    //If you need mailing
    //public SmtpOption Mailing { get; set; } = new();

    public AccountLimits AccountLimit { get; set; } = new();

    public class AccountLimits
    {
        public int CreationEventNumberLimitForEsaeSubscription { get; set; } = 1;
        public int CreationEventNumberLimitForDeraouSubscription { get; set; } = 3;
    }

    public void Check(ILogger logger)
    {
        List<string> list = [];
        if (string.IsNullOrEmpty(BaseUrl))
            list.Add(nameof(BaseUrl));

        if (list.Count > 0)
        {
            string message = $"Misconfiguration:{Environment.NewLine}{string.Join($"{Environment.NewLine}\t", list)}";
            logger.LogCritical(message);
            throw new Exception(message);
        }
    }
}