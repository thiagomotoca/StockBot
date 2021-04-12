using System.Threading.Tasks;

namespace StockBot.Interfaces.Services
{
    public interface ITwitterService
    {
         Task PostTweet(string message);
    }
}