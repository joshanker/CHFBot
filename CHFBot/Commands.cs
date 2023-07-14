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
using SquadronObjects;


namespace BotCommands
{

    public class Commands
    {
       public string getQuote()
        {
            string quote = "quote in getQuote was not populated.";
            try
            {
                var lines = File.ReadAllLines("C:\\quotes.txt");
                var r = new Random();
                var randomLineNumber = r.Next(0, lines.Length - 1);
                var line = lines[randomLineNumber];
                return line;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block of getQuote.");
            }
            return quote;
        }

        public async Task sendQuote(DiscordSocketClient _client) // 1
        {
            //DiscordSocketClient _client = new DiscordSocketClient(); // 2
            ulong id = 342132137064923136; // 3
            var chnl = _client.GetChannel(id) as IMessageChannel; // 4
            //await chnl.SendMessageAsync("Announcement - testing an automated quote!"); // 5
            Console.WriteLine("!quote called for by automated timer");
            Commands getQuote = new Commands();
            string quote = getQuote.getQuote();
            //await message.Channel.SendMessageAsync(quote);
            await chnl.SendMessageAsync(quote);
        }


        async public Task<string> scrapeTitle()
        {
            string title = "title in scrapeTitle was not populated.";
            try
            {
                Console.WriteLine("!scraping title.");

                string url = "https://warthunder.com/en/community/claninfo/Cadet";
                Webscraper scraper = new Webscraper();
                title = await scraper.ScrapeWebsiteTitleAsync(url);
                Console.WriteLine("Website title: " + title);

                return title;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block of scrapeTitle.");
            }
            return title;
        }

        async public Task<string> scrapeBody()
        {
            string body = "body in scrapeBody was not populated.";
            try
            {
                Console.WriteLine("!scraping body.");

                string url = "https://warthunder.com/en/community/claninfo/Cadet";
                Webscraper scraper = new Webscraper();
                body = await scraper.ScrapeWebsiteBodyAsync(url);
                Console.WriteLine("Website body: " + body);

                return body;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block of scrapeBody");
            }
            return body;
        }

        async public Task<string> scrapeAll()
        {
            string all = "all in scrapeAll was not populated.";
            try
            {
                Console.WriteLine("!scraping all.");

                string url = "https://warthunder.com/en/community/claninfo/Cadet";
                Webscraper scraper = new Webscraper();
                all = await scraper.ScrapeWebsiteAllAsync(url);
                Console.WriteLine("Website all: " + all);

                return all;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block of scrapeAll");
            }
            return all;
        }

        async public Task<string> scrapeAllAndPopulate(SquadronObj objname)
        {
            string all = "all in scrapeAllAndPopulate was not populated.";
            try
            {
                Console.WriteLine("!scraping all and populating...");

                string url = "https://warthunder.com/en/community/claninfo/Cadet";
                Webscraper scraper = new Webscraper();
                all = await scraper.ScrapeWebsiteAllAndPopulateAsync(url);
                Console.WriteLine("Website all and populate: " + all);

                return all;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block of scrapeAll");
            }
            return all;
        }


    }
 }
