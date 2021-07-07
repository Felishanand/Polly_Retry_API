using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Retry
{
    class Program
    {
        static readonly string uri = "https://jsonplaceholder.typicode.com/posts";
        private static readonly int retryCount = 2;

        static void Main(string[] args)
        {
            Console.WriteLine("Poly Http Retry Mechanism.");

            GetMockData().GetAwaiter().GetResult();

            Console.ReadKey();
        }

        private static async Task GetMockData()
        {
            HttpClient httpClient = new HttpClient();

            var policy = Policy.Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(retryCount, (ex, retryCnt) =>
                 {
                     Console.WriteLine($"Retry count {retryCnt}");
                 });
                

            Console.WriteLine("Connecting to mock api.");

            HttpResponseMessage response = await policy.ExecuteAsync(() =>httpClient.GetAsync(uri));         
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var posts = JsonConvert.DeserializeObject<List<Post>>(json);

                Console.WriteLine("First Data:");
                Console.WriteLine(posts[0].Title);
            }
            else
            {
                Console.WriteLine($"Not Response:{response.IsSuccessStatusCode}");
            }

        }
    }
}
