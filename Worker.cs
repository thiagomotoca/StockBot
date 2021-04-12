using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using StockBot.Interfaces.Services;

namespace StockBot
{
    [DisallowConcurrentExecution]
    public class Worker : IJob
    {
        private readonly ILogger<Worker> _logger;
        private readonly IYahooService _yahooService;
        private readonly ITwitterService _twitterService;

        private decimal stockPrice = 0;

        public Worker(
            ILogger<Worker> logger,
            IYahooService yahooService,
            ITwitterService twitterService
        )
        {
            _logger = logger;
            _yahooService = yahooService;
            _twitterService = twitterService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Starting Stock Bot at {DateTime.Now}");

            var marketQuote = await _yahooService.GetQuotes("PETR4.SA");

            if (marketQuote.QuoteResponse.Result[0].RegularMarketPrice > stockPrice)
                await _twitterService.PostTweet("");
            else if (marketQuote.QuoteResponse.Result[0].RegularMarketPrice < stockPrice)
                await _twitterService.PostTweet("");
            else
                await _twitterService.PostTweet("");
            
            await Task.CompletedTask;
        }
    }
}
