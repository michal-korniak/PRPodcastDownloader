using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PR1PodcastDownloader
{
    public static class Parser
    {

        public static async Task<List<Podcast>> GetPodcasts(string[] urls)
        {
            var podcasts = new List<Podcast>();
            using (var webClient = new WebClient())
            {
                foreach (var url in urls)
                {
                    try
                    {
                        var source = webClient.DownloadString(url);
                        var context = BrowsingContext.New(Configuration.Default);
                        var document = await context.OpenAsync(req => req.Content(source));

                        var podcast = new Podcast();

                        podcast.Title = ParseTitle(document);
                        podcast.PublishDate = ParsePublishDate(document);
                        podcast.Link = ParseLink(document);

                        podcasts.Add(podcast);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Problem with parsing: {url}");
                        Console.WriteLine(ex);
                    }
                }
                return podcasts;
            }
        }

        private static string ParseLink(IDocument document)
        {
            var documentSource = document.Source.Text;

            string linkEnd = ".mp3";
            int linkEndIndex = documentSource.IndexOf(linkEnd) + linkEnd.Length;
            string linkStart = "static.prsa.pl";
            int linkStartIndex = documentSource.LastIndexOf(linkStart, linkEndIndex);

            string fileUrl = "http://" + documentSource.Substring(linkStartIndex, linkEndIndex - linkStartIndex);

            return fileUrl;
        }
        private static DateTime ParsePublishDate(IDocument document)
        {
            var publishDateElement = document.GetElementById("datetime2");
            if (publishDateElement != null)
            {
                var publishDateString = publishDateElement
                    .InnerHtml
                    .Replace("\n", "")
                    .Trim();
                var publishDate = DateTime.Parse(publishDateString);
                return publishDate;
            }
            else
            {

                var documentSource = document.Source.Text;
                string dateStart = "Data emisji:";
                var dateStartIndex = documentSource.IndexOf(dateStart) + dateStart.Length;

                string dateEnd = "</p>";
                var dateEndIndex = documentSource.IndexOf(dateEnd, dateStartIndex);

                int dateLength = 50;
                string date = documentSource.Substring(dateEndIndex - dateLength, dateLength);
                date = new string(date.Where(x => char.IsDigit(x) || x == '.').ToArray());

                return DateTime.Parse(date);
            }



        }
        private static string ParseTitle(IDocument document)
        {
            string rawTitle = document.GetElementsByClassName("no-border")
                .Select(x => x.GetElementsByClassName("title")
                .FirstOrDefault())
                .Where(x => x != null)
                .FirstOrDefault()
                .InnerHtml;

            string title = new string(rawTitle.Where(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x)).ToArray()).Trim();

            return title;
        }
    }
}