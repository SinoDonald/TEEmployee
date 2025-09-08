using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TEEmployee.Models.Facility
{
    public class PresenceSensorService
    {
        private readonly HttpClient _httpClient;
        private const string AppId = "1413167375177777152edc51";
        private const string KeyId = "K.1413167375207137280";
        private const string AppKey = "6jmlkcax367py1rf5cp9zao6d7tl5kmm";
        private const string Accesstoken = "36f790fbd114c75c1a46974fc0489479";

        public PresenceSensorService()
        {
            _httpClient = new HttpClient();

            // Add the Appid and Keyid to the default request headers
            _httpClient.DefaultRequestHeaders.Add("Appid", AppId);
            _httpClient.DefaultRequestHeaders.Add("Keyid", KeyId);
            _httpClient.DefaultRequestHeaders.Add("Accesstoken", Accesstoken);
            _httpClient.DefaultRequestHeaders.Add("AppKey", AppKey);

            // Generate Time and Nonce (milliseconds since epoch)
            long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();            

            string preSign = $"Accesstoken={Accesstoken}&Appid={AppId}&Keyid={KeyId}&Nonce={nonce}&Time={time}{AppKey}";
            string preSignLower = preSign.ToLowerInvariant();
            string sign = ComputeMD5Hash(preSignLower);

            _httpClient.DefaultRequestHeaders.Add("Time", time.ToString());
            _httpClient.DefaultRequestHeaders.Add("Nonce", nonce.ToString());
            _httpClient.DefaultRequestHeaders.Add("Sign", sign);

            // Optional: clear and set the Accept header for JSON
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetSensorResourceData()
        {
            // Prepare JSON body

            var json = @"{
                ""intent"": ""query.resource.value"",
                ""data"": {
                    ""resources"": [
                        { ""subjectId"": ""lumi1.54ef4479c92f"", ""resourceIds"": [ ""3.51.85"" ] },
                        { ""subjectId"": ""lumi1.54ef4479cf30"", ""resourceIds"": [ ""3.51.85"" ] },
                        { ""subjectId"": ""lumi1.54ef4479d54a"", ""resourceIds"": [ ""3.51.85"" ] }
                    ]
                }
            }";

            
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://open-usa.aqara.com/v3.0/open/api", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private static string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (var b in data)
                    sb.Append(b.ToString("x2")); // "x2" => lowercase hex
                return sb.ToString();
            }
        }
    }
}