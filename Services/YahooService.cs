using System;
using System.Net.Http;
using System.Threading.Tasks;
using StockBot.Interfaces.Services;
using StockBot.Model;

namespace StockBot.Services
{
    public class YahooService : IYahooService
    {
        private readonly HttpClient _client;

        public YahooService(WorkerOptions workerOptions)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(workerOptions.YahooFinanceApi.ApiUrl);
            _client.DefaultRequestHeaders.Add("x-rapidapi-key", workerOptions.YahooFinanceApi.ApiKey);
            _client.DefaultRequestHeaders.Add("x-rapidapi-host", workerOptions.YahooFinanceApi.ApiHost);
        }

        public async Task<QuoteResponseDto> GetQuotes(string symbol)
        {
            var response = await _client.GetAsync($"market/v2/get-quotes?symbols={symbol}&region=BR");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<QuoteResponseDto>();
            return result;
        }
    }
}