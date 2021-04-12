namespace StockBot.Model
{
    public class QuoteResponseDto
    {
        public QuoteResponse QuoteResponse { get; set; }
    }

    public class QuoteResponse
    {
        public Result[] Result { get; set; }
    }

    public class Result
    {
        public decimal RegularMarketPrice { get; set; }
    }
}