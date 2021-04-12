using System.Threading.Tasks;
using StockBot.Model;

namespace StockBot.Interfaces.Services
{
    public interface IYahooService
    {
         Task<QuoteResponseDto> GetQuotes(string symbol);
    }
}