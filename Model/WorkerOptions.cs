namespace StockBot.Model
{
    public class WorkerOptions
    {
        public YahooFinanceApi YahooFinanceApi { get; set; }
        public TwitterApi TwitterApi { get; set; }
    }

    public class YahooFinanceApi
    {
        public string ApiKey { get; set; }
        public string ApiHost { get; set; }
        public string ApiUrl { get; set; }
    }

    public class TwitterApi
    {
        public string ApiUrl { get; set; }
        public string AuthConsumerKey { get; set; }
        public string AuthConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}