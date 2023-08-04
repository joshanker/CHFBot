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
using System.Globalization;


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
                //Console.WriteLine("Website all and populate.");

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

        public async void printPlayers(IMessageChannel chnl, SquadronObj sqdobj)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Player player in sqdobj.Players)
            {
                sb.AppendLine($"{player.Number}: {player.PlayerName,-20} SQB Points: {player.PersonalClanRating}");
            }
            sqdobj.allsqd = sb.ToString();
            string longContent = sqdobj.allsqd;
            await SendLongContentAsEmbedAsync(chnl, longContent);
        }

        public async Task PrintTop20Players(IMessageChannel chnl, SquadronObj sqdobj, List<Player> top20Players)
        {
            StringBuilder sb = new StringBuilder();
            int totalScore = 0;
            for (int i = 0; i < top20Players.Count; i++)
            {
                Player player = top20Players[i];
                sb.AppendLine($"{i + 1}. {player.PlayerName} (Score: {player.PersonalClanRating})");
                totalScore = totalScore + player.PersonalClanRating;
                
            }
            sb.AppendLine();
            sb.AppendLine("Total score of Top20: " + totalScore.ToString());
            string longContent = sb.ToString();
            await SendLongContentAsEmbedAsync(chnl, longContent);
        }

        public async void printSum(IMessageChannel chnl, SquadronObj sqdobj)
        {
            //foreach (Player player in AcadObj.Players)
            //{
            //    await chnl.SendMessageAsync("Name: " + player.PlayerName + " \nNumber: " + player.Number + " \nPersonal Clan Rating: " + player.PersonalClanRating + " \nActivity: " + player.Activity + " \nRole: " + player.Rank + " \nDate of Entry: " + player.DateOfEntry + "\n-");
            //}
            int sum = 0;
            //StringBuilder sb = new StringBuilder();
            foreach (Player player in sqdobj.Players)
            {

                //This works just fine... It prints everything.  Commented out because I only want names/points here.
                //sb.Append("Name: " + player.PlayerName + " \nNumber: " + player.Number + " \nPersonal Clan Rating: " + player.PersonalClanRating + " \nActivity: " + player.Activity + " \nRole: " + player.Rank + " \nDate of Entry: " + player.DateOfEntry + "\n-\n");
                sum = sum + player.PersonalClanRating;
                //sb.AppendLine($"{player.Number}: {player.PlayerName,-20} SQB Points: {player.PersonalClanRating}");
            }
            sqdobj.totalRating = sum;

            await chnl.SendMessageAsync("Total Squadron Score: " + sqdobj.totalRating);


            //var embedBuilder = new EmbedBuilder();
            //embedBuilder.Description = sqdobj.allsqd;


            //await chnl.SendMessageAsync(embed: embedBuilder.Build());
            //await chnl.SendMessageAsync(sqdobj.allsqd);

        }

        public static async Task SendLongContentAsEmbedAsync(IMessageChannel channel, string content)
        {
            const int maxEmbedLength = 4096;
            const int maxChunkLength = 2000;

            if (content.Length <= maxEmbedLength)
            {
                // If the content fits within the limit, send it as a single embedded message
                await channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription($"```{content}```").Build());
            }
            else
            {
                // Split the content into chunks of maxChunkLength
                List<string> chunks = new List<string>();
                StringBuilder currentChunk = new StringBuilder();

                foreach (string line in content.Split('\n'))
                {
                    if (currentChunk.Length + line.Length + 6 <= maxChunkLength) // Adding 2 for the newline characters that will be added later
                    {

                        currentChunk.Append(line);
                    }
                    else
                    {
                        chunks.Add(currentChunk.ToString());
                        currentChunk.Clear();

                    }
                }

                // Add the last chunk
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString());
                }

                // Send each chunk as a separate embedded message
                for (int i = 0; i < chunks.Count; i++)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                        .WithDescription($"```{chunks[i]}```")
                        .WithFooter($"Chunk {i + 1}/{chunks.Count}");

                    await channel.SendMessageAsync(embed: embedBuilder.Build());
                }
                await channel.SendMessageAsync("End of squadron printout.").ConfigureAwait(true);

            }
        }

        public SquadronObj validateSquadron(string input)
        {
            string cadetUrl = "https://warthunder.com/en/community/claninfo/Cadet";
            string BofSsUrl = "https://warthunder.com/en/community/claninfo/Band%20Of%20Scrubs";
            string AcademyUrl = "https://warthunder.com/en/community/claninfo/The%20Academy";

            string url = "not yet set...";
            SquadronObj squadronObject = new SquadronObj(input, url);

            if (input == "Cadet")
            {
                url = cadetUrl;
            }
            if (input == "BofSs")
            {
                url = BofSsUrl;
            }
            if (input == "Academy")
            {
                url = AcademyUrl;
            }
            if(url == "not yet set...")
            {
                squadronObject.isValidSquadron = false;
                return squadronObject;
            }
            else
            {
                squadronObject.isValidSquadron = true;
                squadronObject.url = url;
                squadronObject.SquadronName = input;
                return squadronObject;
            }
        }

        public async Task<SquadronObj> populateScore(SquadronObj sqdobj)
        {
            
            
                try
                {
                    Console.WriteLine("Populating score...");

                    //string url = "https://warthunder.com/en/community/claninfo/Cadet";
                    //string url = objname.url;
                    Webscraper scraper = new Webscraper();
                    sqdobj = await scraper.scrapeWebsiteAndPopulateScoreAsync(sqdobj);
                    //Console.WriteLine("Website all and populate.");

                    return sqdobj;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                return null;
                }
                
        }
       
        public SquadronObj PopulateSquadronFromTextFile(string filePath)
        {
            SquadronObj squadronObj = new SquadronObj();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Squadron: "))
                    {
                        squadronObj.SquadronName = line.Substring("Squadron: ".Length);
                    }
                    else if (line.StartsWith("Player Count: "))
                    {
                        int playerCount;
                        if (int.TryParse(line.Substring("Player Count: ".Length), out playerCount))
                        {
                            squadronObj.Players = new List<Player>(playerCount);
                        }
                    }
                    else if (line.StartsWith("Score: "))
                    {
                        int score;
                        if (int.TryParse(line.Substring("Score: ".Length), out score))
                        {
                            squadronObj.Score = score;
                        }
                    }
                    else if (line.StartsWith("Name: "))
                    {
                        Player player = new Player();
                        player.PlayerName = line.Substring("Name: ".Length);
                        line = reader.ReadLine(); // Read the next line
                        if (line.StartsWith("Number: "))
                        {
                            int number;
                            if (int.TryParse(line.Substring("Number: ".Length), out number))
                            {
                                player.Number = number;
                            }
                        }
                        line = reader.ReadLine(); // Read the next line
                        if (line.StartsWith("Personal Clan Rating: "))
                        {
                            int rating;
                            if (int.TryParse(line.Substring("Personal Clan Rating: ".Length), out rating))
                            {
                                player.PersonalClanRating = rating;
                            }
                        }
                        line = reader.ReadLine(); // Read the next line
                        if (line.StartsWith("Activity: "))
                        {
                            player.Activity = line.Substring("Activity: ".Length);
                        }
                        line = reader.ReadLine(); // Read the next line
                        if (line.StartsWith("Role: "))
                        {
                            player.Rank = line.Substring("Role: ".Length);
                        }
                        line = reader.ReadLine(); // Read the next line
                        if (line.StartsWith("Date of Entry: "))
                        {
                            DateTime date;
                            if (DateTime.TryParse(line.Substring("Date of Entry: ".Length), out date))
                            {
                                player.DateOfEntry = date;
                            }
                        }


                        line = reader.ReadLine(); // Read the delimiter line "-------------------------"

                        squadronObj.Players.Add(player);
                    }
                }
            }
            return squadronObj;
        }


        public async Task CompareSquadronFiles(IMessageChannel chnl, string filePath1, string filePath2)
        {
            SquadronObj squadron1 = PopulateSquadronFromTextFile(filePath1);
            SquadronObj squadron2 = PopulateSquadronFromTextFile(filePath2);

            if (squadron1 == null || squadron2 == null)
            {
                // Error reading the files or creating the SquadronObj objects
                return;
            }

            List<string> joiners = new List<string>();
            List<string> leavers = new List<string>();

            foreach (Player player in squadron2.Players)
            {
                bool found = false;
                foreach (Player player2 in squadron1.Players)
                {
                    if (player.PlayerName == player2.PlayerName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    leavers.Add(player.PlayerName);
                }
            }

            foreach (Player player in squadron1.Players)
            {
                bool found = false;
                foreach (Player player2 in squadron2.Players)
                {
                    if (player.PlayerName == player2.PlayerName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    joiners.Add(player.PlayerName);
                }
            }

            await SendJoinersAndLeaversAsync(chnl, joiners, leavers);
        }


        private async Task SendJoinersAndLeaversAsync(IMessageChannel chnl, List<string> joiners, List<string> leavers)
        {
            // Create and send the messages
            StringBuilder messageBuilder = new StringBuilder();

            if (joiners.Count > 0)
            {
                messageBuilder.AppendLine("New joiners:");
                messageBuilder.AppendLine(string.Join("\n", joiners));
                messageBuilder.AppendLine(); // Add an empty line for spacing
            }

            if (leavers.Count > 0)
            {
                messageBuilder.AppendLine("Leavers:");
                messageBuilder.AppendLine(string.Join("\n", leavers));
            }

            await chnl.SendMessageAsync(messageBuilder.ToString());
        }



        //private async Task SendJoinersAndLeaversAsync(IMessageChannel chnl, List<string> joiners, List<string> leavers)
        //{
        //    // Create and send the messages
        //    if (joiners.Count > 0)
        //    {
        //        StringBuilder joinersMessage = new StringBuilder("New joiners:\n");
        //        foreach (string joiner in joiners)
        //        {
        //            joinersMessage.AppendLine(joiner);
        //        }
        //        await chnl.SendMessageAsync(joinersMessage.ToString());
        //    }

        //    if (leavers.Count > 0)
        //    {
        //        StringBuilder leaversMessage = new StringBuilder("Leavers:\n");
        //        foreach (string leaver in leavers)
        //        {
        //            leaversMessage.AppendLine(leaver);
        //        }
        //        await chnl.SendMessageAsync(leaversMessage.ToString());
        //    }
        //}




    }
 }
