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
        
        //ulong id = 1125693277295886357; // 3
        //IMessageChannel chnl = _client.GetChannel(id) as IMessageChannel; // 4


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
            ulong id = 1125693277295886357; // 3
            var chnl = _client.GetChannel(id) as IMessageChannel; // 4
            //await chnl.SendMessageAsync("Announcement - testing an automated quote!"); // 5
            Console.WriteLine("!quote called for by automated timer");
            Commands getQuote = new Commands();
            string quote = getQuote.getQuote();
            //await message.Channel.SendMessageAsync(quote);
            await chnl.SendMessageAsync(quote);
        }

        public async Task<SquadronObj> scrapeAllAndPopulate(SquadronObj objname)
        {
            //string all = "all in scrapeAllAndPopulate was not populated.";
            try
            {
                Console.WriteLine("!scraping all and populating...");

                //string url = "https://warthunder.com/en/community/claninfo/Cadet";
                //string url = objname.url;
                Webscraper scraper = new Webscraper();
                objname = await scraper.ScrapeWebsiteAllAndPopulateAsync(objname);
                Console.WriteLine("Website all and populate.");

                return objname;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                //Console.WriteLine("Executing finally block of scrapeAll");
            }
            return objname;
        }


        public async void printPlayers(IMessageChannel chnl,SquadronObj sqdobj)
        {
            //foreach (Player player in AcadObj.Players)
            //{
            //    await chnl.SendMessageAsync("Name: " + player.PlayerName + " \nNumber: " + player.Number + " \nPersonal Clan Rating: " + player.PersonalClanRating + " \nActivity: " + player.Activity + " \nRole: " + player.Rank + " \nDate of Entry: " + player.DateOfEntry + "\n-");
            //}
                        
            StringBuilder sb = new StringBuilder();
            foreach (Player player in sqdobj.Players)
            {

                //This works just fine... It prints everything.  Commented out because I only want names/points here.
                //sb.Append("Name: " + player.PlayerName + " \nNumber: " + player.Number + " \nPersonal Clan Rating: " + player.PersonalClanRating + " \nActivity: " + player.Activity + " \nRole: " + player.Rank + " \nDate of Entry: " + player.DateOfEntry + "\n-\n");
                
                sb.Append("Name: " + player.PlayerName + "          Personal Clan Rating: " + player.PersonalClanRating + " \n-\n");

            }
            sqdobj.allsqd = sb.ToString();
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Description = sqdobj.allsqd;


            await chnl.SendMessageAsync(embed: embedBuilder.Build());
            //await chnl.SendMessageAsync(sqdobj.allsqd);


        }


    }
 }
