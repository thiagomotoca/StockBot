using System.Collections.Generic;

namespace StockBot.Model
{
    public class QuoteResponseDto
    {
        public QuoteResponse QuoteResponse { get; set; }
    }

    public class QuoteResponse
    {
        public List<Result> Result { get; set; }
    }

    public class Result
    {
        public string Symbol { get; set; }
        public decimal RegularMarketPrice { get; set; }
        public decimal RegularMarketPreviousClose { get; set; }
    }
}