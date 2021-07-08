using Newtonsoft.Json;
using Polly;
using Polly.Bulkhead;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BulkHead
{
    class Program
    {
        static string requestEndPoint = "https://jsonplaceholder.typicode.com/posts6";
        static HttpClient httpClient = new HttpClient();
        static AsyncBulkheadPolicy<HttpResponseMessage> bulkHeadIsolationPolicy;

        static void Main(string[] args)
        {
            Console.WriteLine("Bulk Head");

            Setup();

            List<Task> taskList = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run(() => Fetch(i)));
            }

            Task.WaitAll(taskList.ToArray());

        }

        private static void Setup()
        {
            bulkHeadIsolationPolicy = Policy.BulkheadAsync<HttpResponseMessage>(2, 2, OnBulkHeadRejectedAsync);
        }

        private static Task OnBulkHeadRejectedAsync(Context arg)
        {
            Console.WriteLine($"OnBulkHeadRejectedAsync Excuted");
            return Task.CompletedTask;
        }

        private static void LogBulkHeadInfo()
        {
            Console.WriteLine($"BulkheadAvailableCount" +
                $"{bulkHeadIsolationPolicy.BulkheadAvailableCount}");

            Console.WriteLine($"QueueAvailableCount" +
                $"{bulkHeadIsolationPolicy.QueueAvailableCount}");
        }

        private static async Task Fetch(int id)
        {
            try
            {
                LogBulkHeadInfo();

                HttpResponseMessage response = await bulkHeadIsolationPolicy.ExecuteAsync(
                    () =>
                    {
                        return httpClient.GetAsync(requestEndPoint + "/" + id);
                    });

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var post = JsonConvert.DeserializeObject<Post>(json);
                    Console.WriteLine($"\n {post.Title}");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
