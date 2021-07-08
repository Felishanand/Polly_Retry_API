using Newtonsoft.Json;
using Polly;
using Polly.Bulkhead;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BulkHead
{
    class Program : ProgramBase
    {
        static readonly string requestEndPoint = "https://jsonplaceholder.typicode.com/posts";
        private static readonly int retryCount = 2;
        //static BulkheadPolicy<HttpResponseMessage> bulkHeadIsolationPolicy;
        private static AsyncBulkheadPolicy<HttpResponseMessage> bulkHeadIsolationPolicy;

        static void Main(string[] args)
        {
            Console.WriteLine("Bulk Head Operation");

            Setup();

            List<Task> taskList = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run(() => GetMockData(i)));
            }

            Task.WaitAll(taskList.ToArray());
        }

        private static void Setup()
        {
            bulkHeadIsolationPolicy = Policy.BulkheadAsync<HttpResponseMessage>(2, 2, onBulkheadRejectedAsync);
        }

        private static void LogBulkHeadInfo()
        {
            Console.WriteLine($"BulkheadAvailableCount:"
                + $"{ bulkHeadIsolationPolicy.BulkheadAvailableCount}");

            Console.WriteLine($"QueueAvailableCount:"
                + $"{ bulkHeadIsolationPolicy.QueueAvailableCount}");

        }

        private static async Task GetMockData(int id)
        {
            LogBulkHeadInfo();
           
            HttpClient httpClient = new HttpClient();           

            HttpResponseMessage response = await bulkHeadIsolationPolicy.ExecuteAsync(
            () =>
            {
                return httpClient.GetAsync(requestEndPoint + "/" + id);
            });
            
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var posts = JsonConvert.DeserializeObject<Post>(json);

                    Console.WriteLine("Result Data:");
                    Console.WriteLine(posts.Title);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

        }
    }
}
