using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper
{
    class Program
    {
        private static string urlFirst = "<a class=\"business-name\" href=\"";
        private static string urlSecond = "\" data-analytics=";
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            Console.WriteLine("Scraper Init!");
            var baseUrl = ConfigurationManager.AppSettings.Get("baseUrl");
            var outputPath = ConfigurationManager.AppSettings.Get("outputPath");
            var search = ConfigurationManager.AppSettings.Get("search");
            var timeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("timeout"));
            Console.WriteLine("Config:");
            Console.WriteLine("Base URL: " + baseUrl);
            Console.WriteLine("Output Path " + outputPath);
            Console.WriteLine("Search query string: " + search);
            Console.WriteLine("Timeout between requests: " + timeout);
            var page = 1;
            while (true)
            {
                Console.WriteLine("Starting new page...");
                var response = await client.GetAsync(baseUrl + search + page.ToString(), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Request failed!");
                var content = await response.Content.ReadAsStringAsync();
                var urls = GetUrls(content);
                var records = new List<Record>();
                foreach (var url in urls)
                {
                    if (timeout > 0)
                        Thread.Sleep(timeout);
                    var res = await client.GetAsync(baseUrl + url, HttpCompletionOption.ResponseContentRead);
                    var data = await res.Content.ReadAsStringAsync();
                    records.Add(new Record(data));
                }
                Console.WriteLine("Saving records...");
                var prevContent = File.Exists(outputPath)
                    ? JsonConvert.DeserializeObject<List<Record>>(await File.ReadAllTextAsync(outputPath))
                    : new List<Record>();
                var newContent = prevContent.Union(records);
                await File.WriteAllTextAsync(outputPath, JsonConvert.SerializeObject(newContent), Encoding.UTF8);
                Console.WriteLine("Saving complete!");
                Console.WriteLine($"Total records: {newContent.Count()}");
                page += 1;
            }
        }
        private static List<string> GetUrls(string html)
        {
            var parts = html.Split(urlFirst);
            var urls = new List<string>();
            foreach (var item in parts)
            {
                if (!item.Contains(urlSecond))
                    continue;
                var url = item.Split(urlSecond)[0];
                if (url.Length > 100)
                    continue;
                urls.Add(url);
            }
            return urls;
        }
    }
}
