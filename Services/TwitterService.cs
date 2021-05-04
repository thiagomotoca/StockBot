using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockBot.Interfaces.Services;
using StockBot.Model;

namespace StockBot.Services
{
    public class TwitterService : ITwitterService
    {
        private readonly ILogger<TwitterService> _logger;
        private readonly string _consumerKey;
        private readonly string _consumerKeySecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
        private readonly string _twitterApiUrl;
        private readonly HMACSHA1 _sigHasher;
        private readonly DateTime epochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TwitterService(
            ILogger<TwitterService> logger,
            WorkerOptions workerOptions
        )
        {
            _logger = logger;

            _consumerKey = workerOptions.TwitterApi.AuthConsumerKey;
            _consumerKeySecret = workerOptions.TwitterApi.AuthConsumerSecret;
            _accessToken = workerOptions.TwitterApi.AccessToken;
            _accessTokenSecret = workerOptions.TwitterApi.AccessTokenSecret;
            _twitterApiUrl = workerOptions.TwitterApi.ApiUrl;

            _sigHasher = new HMACSHA1(new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", _consumerKeySecret, _accessTokenSecret)));
        }

        public async Task PostTweet(string message)
        {
            var data = new Dictionary<string, string> {
                { "status", message },
                { "trim_user", "1" }
            };

            var timestamp = (int)((DateTime.UtcNow - epochUtc).TotalSeconds);
            var url = $"{_twitterApiUrl}statuses/update.json";

            data.Add("oauth_consumer_key", _consumerKey);
            data.Add("oauth_signature_method", "HMAC-SHA1");
            data.Add("oauth_timestamp", timestamp.ToString());
            data.Add("oauth_nonce", "a");
            data.Add("oauth_token", _accessToken);
            data.Add("oauth_version", "1.0");

            var sigString = string.Join(
                "&",
                data.Union(data)
                    .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            var fullSigData = string.Format(
                "{0}&{1}&{2}",
                "POST",
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(sigString.ToString())
            );

            data.Add("oauth_signature", Convert.ToBase64String(_sigHasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData.ToString()))));

            string oAuthHeader = "OAuth " + string.Join(
                ", ",
                data.Where(kvp => kvp.Key.StartsWith("oauth_"))
                    .Select(kvp => string.Format("{0}=\"{1}\"", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            var formData = new FormUrlEncodedContent(data.Where(kvp => !kvp.Key.StartsWith("oauth_")));

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Authorization", oAuthHeader);

                var httpResp = await http.PostAsync(url, formData);
                var respBody = await httpResp.Content.ReadAsStringAsync();

                _logger.LogInformation(respBody);
            }
        }
    }
}