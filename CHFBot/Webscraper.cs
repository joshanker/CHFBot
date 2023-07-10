using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Scraper;
using System.Timers;
using Discord;
using Discord.WebSocket;

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

        public async Task<string> ScrapeWebsiteBodyAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(htmlContent);

                HtmlNode bodyNode = document.DocumentNode.SelectSingleNode("//body");

                if (bodyNode != null)
                {
                    string body = bodyNode.InnerText.Trim();
                    return body;
                }

                return "body not found";
            }
        }

        public async Task<string> ScrapeWebsiteAllAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//table[@class='wt-clanlist-table']/tbody/tr");


                for (int i = 8; i < 771; i = i+6)
                {

                    string xpath = "//*[@id=\"bodyRoot\"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + i + "]/a";

                    HtmlNode n1 = doc.DocumentNode.SelectSingleNode(xpath);
                    String n1t = n1.InnerText;
                    Console.WriteLine(n1t);

                }
            


                if (rows != null)
                {
                    foreach (HtmlNode row in rows)
                    {
                        string name = row.SelectSingleNode(".//td[1]").InnerText.Trim();
                        string activity = row.SelectSingleNode(".//td[2]").InnerText.Trim();
                        string role = row.SelectSingleNode(".//td[3]").InnerText.Trim();
                        string rating = row.SelectSingleNode(".//td[4]").InnerText.Trim();
                        string dateOfEntry = row.SelectSingleNode(".//td[5]").InnerText.Trim();

                        
                        Console.WriteLine("Name: " + name);
                        Console.WriteLine("Activity: " + activity);
                        Console.WriteLine("Role: " + role);
                        Console.WriteLine("Rating: " + rating);
                        Console.WriteLine("Date of Entry: " + dateOfEntry);
                        Console.WriteLine();

                        return "I hit the return in the if";
                    }
                }
                else
                {
                    Console.WriteLine("No data found.");
                    return "I hit the return in the else";
                }

                Console.ReadLine();
                return "blah blah";
                
            }
        }


    }
}
