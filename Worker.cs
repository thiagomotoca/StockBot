using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Quartz;
using StockBot.Interfaces.Services;
using StockBot.Model;

namespace StockBot
{
    [DisallowConcurrentExecution]
    public class Worker : IJob
    {
        private readonly ILogger<Worker> _logger;
        private readonly IYahooService _yahooService;
        private readonly ITwitterService _twitterService;
        private readonly IMemoryCache _memoryCache;

        private int startingHour;
        private int finalHour;
        private List<string> symbols;

        public Worker(
            ILogger<Worker> logger,
            IYahooService yahooService,
            ITwitterService twitterService,
            IMemoryCache memoryCache,
            WorkerOptions workerOptions
        )
        {
            _logger = logger;
            _yahooService = yahooService;
            _twitterService = twitterService;
            _memoryCache = memoryCache;

            startingHour = workerOptions.StartingHour;
            finalHour = workerOptions.FinalHour;
            symbols = workerOptions.Symbols;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int hourNow = DateTime.Now.Hour;
            if (hourNow < startingHour || hourNow > finalHour)
                await Task.CompletedTask;

            _logger.LogInformation($"Starting Stock Bot at {DateTime.Now}");

            var symbolsString = string.Join(',', symbols);
            var marketQuote = await _yahooService.GetQuotes(symbolsString);

            foreach (var symbol in symbols)
            {
                var stockPrice = _memoryCache.GetOrCreate<decimal>(
                    symbol,
                    cacheEntry => 
                        {
                            return marketQuote.QuoteResponse.Result.First(r => r.Symbol == symbol).RegularMarketPreviousClose;
                        });
                
                var newStockPrice = marketQuote.QuoteResponse.Result.First(r => r.Symbol == symbol)?.RegularMarketPrice;

                if (stockPrice > 0)
                {
                    if (newStockPrice > stockPrice)
                        await _twitterService.PostTweet($"{symbol} subiu - R${newStockPrice}");
                    else if (newStockPrice < stockPrice)
                        await _twitterService.PostTweet($"{symbol} baixou - R${newStockPrice}");
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(1)
                };
                
                _memoryCache.Set(symbol, newStockPrice, cacheEntryOptions);
            }
            
            await Task.CompletedTask;
        }
    }
}
