using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PR1PodcastDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now} - Pobieranie metadanych");
            string[] urls = await File.ReadAllLinesAsync("urls.txt");
            var podcasts = await Parser.GetPodcasts(urls);

            string outputDirectory = "output";
            Directory.CreateDirectory(outputDirectory);

            int number = 1;
            foreach (var podcast in podcasts)
            {
                using var webClient = new WebClient();
                AddUserAgentHeader(webClient);

                var numberWithPads = string.Format(string.Format("{0:D3}", number));
                string fileName = $"{numberWithPads}_{podcast.Title}.mp3";

                string path = $"{outputDirectory}/{fileName}";

                Console.WriteLine($"{DateTime.Now} - Pobieranie pliku: {fileName}");
                await webClient.DownloadFileTaskAsync(podcast.Link, path);

                number += 1;
            }


            Console.WriteLine($"{DateTime.Now} - Pobieranie zakończone");
            Console.ReadKey();

        }

        private static void AddUserAgentHeader(WebClient webClient)
        {
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                    "Windows NT 5.2; .NET CLR 1.0.3705;)");
        }
    }
}
