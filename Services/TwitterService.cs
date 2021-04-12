using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using StockBot.Interfaces.Services;
using StockBot.Model;

namespace StockBot.Services
{
    public class TwitterService : ITwitterService
    {
        private string oAuthConsumerKey;
        private string oAuthConsumerSecret;
        private string accessToken;
        private string accessTokenSecret;
        private string oAuthUrl;

        public TwitterService(WorkerOptions workerOptions)
        {
            oAuthConsumerKey = workerOptions.TwitterApi.AuthConsumerKey;
            oAuthConsumerSecret = workerOptions.TwitterApi.AuthConsumerSecret;
            accessToken = workerOptions.TwitterApi.AccessToken;
            accessTokenSecret = workerOptions.TwitterApi.AccessTokenSecret;
            oAuthUrl = workerOptions.TwitterApi.ApiUrl;
        }

        private string GenerateNonce()
        {
            string nonce = string.Empty;
            var rand = new Random();
            int next = 0;
            for (var i = 0; i < 32; i++)
            {
                next = rand.Next(65, 90);
                char c = Convert.ToChar(next);
                nonce += c;
            }

            return nonce;
        }

        public double GenerateTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        private string GenerateOauthSignature(string status, string nonce, string timestamp)
        {
            string signatureMethod = "HMAC-SHA1";
            string version = "1.0";
            string result = string.Empty;
            string dst = string.Empty;

            dst += string.Format("oauth_consumer_key={0}&", Uri.EscapeDataString(oAuthConsumerKey));
            dst += string.Format("oauth_nonce={0}&", Uri.EscapeDataString(nonce));
            dst += string.Format("oauth_signature_method={0}&", Uri.EscapeDataString(signatureMethod));
            dst += string.Format("oauth_timestamp={0}&", timestamp);
            dst += string.Format("oauth_token={0}&", Uri.EscapeDataString(accessToken));
            dst += string.Format("oauth_version={0}&", Uri.EscapeDataString(version));
            dst += string.Format("status={0}", Uri.EscapeDataString(status));

            string signingKey = string.Empty;
            signingKey = string.Format("{0}&{1}", Uri.EscapeDataString(oAuthConsumerSecret), Uri.EscapeDataString(accessTokenSecret));

            result += "POST&";
            result += Uri.EscapeDataString(oAuthUrl);
            result += "&";
            result += Uri.EscapeDataString(dst);

            var hmac = new HMACSHA1();
            hmac.Key = Encoding.UTF8.GetBytes(signingKey);

            byte[] databuff = System.Text.Encoding.UTF8.GetBytes(result);
            byte[] hashbytes = hmac.ComputeHash(databuff);

            return Convert.ToBase64String(hashbytes);
        }

        private string GenerateAuthorizationHeader(string status)
        {
            string signatureMethod = "HMAC-SHA1";
            string version = "1.0";
            string nonce = GenerateNonce();
            double timestamp = ConvertToUnixTimestamp(DateTime.Now);
            string dst = string.Empty;

            dst = string.Empty;
            dst += "OAuth ";
            dst += string.Format("oauth_consumer_key=\"{0}\", ", Uri.EscapeDataString(oAuthConsumerKey));
            dst += string.Format("oauth_nonce=\"{0}\", ", Uri.EscapeDataString(nonce));
            dst += string.Format("oauth_signature=\"{0}\", ", Uri.EscapeDataString(GenerateOauthSignature(status, nonce, timestamp.ToString())));
            dst += string.Format("oauth_signature_method=\"{0}\", ", Uri.EscapeDataString(signatureMethod));
            dst += string.Format("oauth_timestamp=\"{0}\", ", timestamp);
            dst += string.Format("oauth_token=\"{0}\", ", Uri.EscapeDataString(accessToken));
            dst += string.Format("oauth_version=\"{0}\"", Uri.EscapeDataString(version));
            return dst;
        }

        public async Task PostTweet(string message)
        {
            var authHeader = GenerateAuthorizationHeader(message);
            var postBody = "status=" + Uri.EscapeDataString(message);

            var request = (HttpWebRequest)WebRequest.Create(oAuthUrl);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.UserAgent = "OAuth gem v0.4.4";
            request.Host = "api.twitter.com";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            request.ServicePoint.Expect100Continue = false;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var stream = await request.GetRequestStreamAsync())
            {
                byte[] content = Encoding.UTF8.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            var response = request.GetResponse();
            response.Close();
        }
    }
}