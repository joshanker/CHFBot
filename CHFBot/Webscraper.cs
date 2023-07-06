using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scraper
{
    public class Webscraper
    {
        public Webscraper()
        {
            // Constructor logic here
        }

        public async Task<string> ScrapeWebsiteTitleAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(htmlContent);

                HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//title");

                if (titleNode != null)
                {
                    string title = titleNode.InnerText.Trim();
                    return title;
                }

                return "Title not found";
            }
        }
    }
}
