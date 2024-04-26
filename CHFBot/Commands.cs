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
using Newtonsoft.Json;

namespace BotCommands
{

    public class Commands
    {

        public string getQuote()
        {
            string quote = "quote in getQuote was not populated.";
            try
            {
                var lines = File.ReadAllLines("quotes.txt");
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

            ulong id = 1125693277295886357; // 3
            var chnl = _client.GetChannel(id) as IMessageChannel; // 4

            Console.WriteLine("!quote called for by automated timer");
            Commands getQuote = new Commands();
            string quote = getQuote.getQuote();

            await chnl.SendMessageAsync(quote);
        }

        public async Task<SquadronObj> scrapeAllAndPopulate(SquadronObj objname)
        {
            //string all = "all in scrapeAllAndPopulate was not populated.";
            try
            {
                Console.WriteLine("!scraping all and populating..." + DateTime.Now);

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
                sb.AppendLine($"{player.Number}: {player.PlayerName,-20} Pts: {player.PersonalClanRating}");
            }
            sqdobj.allsqd = sb.ToString();
            string longContent = sqdobj.allsqd;
            await SendLongContentAsEmbedAsync(chnl, longContent);
        }

        public async void printPlayersOverUnder(IMessageChannel chnl, SquadronObj sqdobj, String overUnder, int points)
        {
            overUnder = overUnder.ToLower();
            StringBuilder sb = new StringBuilder();

            // Sort the players by number of points, from most to least
            var sortedPlayers = sqdobj.Players.OrderByDescending(player => player.PersonalClanRating);
            sb.AppendLine($"{" ",-50}");
            foreach (Player player in sortedPlayers)
            {

                if (overUnder == "over")
                {
                    if (player.PersonalClanRating >= points)
                    {
                        sb.AppendLine($"Pts: {player.PersonalClanRating} {player.PlayerName} ");
                    }
                }
                if (overUnder == "under")
                {
                    if (player.PersonalClanRating <= points)
                    {
                        sb.AppendLine($"Pts: {player.PersonalClanRating} {player.PlayerName,-18}");
                    }
                }

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
                sb.AppendLine($"{i + 1}. {player.PlayerName,-20} (Score: {player.PersonalClanRating})");
                totalScore = totalScore + player.PersonalClanRating;
            }
            sb.AppendLine();
            sb.AppendLine("Total score of Top20: " + totalScore.ToString() + " / " + sqdobj.Score);
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

        }

        public async Task SendLongContentAsEmbedAsync(IMessageChannel channel, string content)
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
            string EarlyUrl = "https://warthunder.com/en/community/claninfo/EARLY";
            string RO6Url = "https://warthunder.com/en/community/claninfo/Revenge%20of%20Six";
            string AVR = "https://warthunder.com/en/community/claninfo/AVANGARD";
            string ILWI = "https://warthunder.com/en/community/claninfo/LIGHTWAY";
            string iNut = "https://warthunder.com/en/community/claninfo/Team%20iNut";
            string SKAL = "https://warthunder.com/en/community/claninfo/SKAL%20-%20Pirates%20of%20the%s0North";
            string NEURO = "https://warthunder.com/en/community/claninfo/NEURO";
            string LEDAC = "https://warthunder.com/en/community/claninfo/La%20Legion%20d%20Acier";
            string B0AR = "https://warthunder.com/en/community/claninfo/MAD%20BOARS";
            string SOFUA = "https://warthunder.com/en/community/claninfo/Welcome%20to%20Ukraine";
            string WeBak = "https://warthunder.com/en/community/claninfo/NIKE%20x%20UzBeK";

            string url = "not yet set...";
            SquadronObj squadronObject = new SquadronObj(input, url);

            var urlMap = new Dictionary<string, string>
                {
                    { "Cadet", cadetUrl },
                    { "BofSs", BofSsUrl },
                    { "Academy", AcademyUrl },
                    { "Early", EarlyUrl },
                    { "RO6", RO6Url },
                    { "AVR", AVR },
                    { "ILWI", ILWI },
                    { "iNut",  iNut},
                    { "SKAL",  SKAL},
                    { "NEURO",  NEURO},
                    { "LEDAC",  LEDAC},
                    { "B0AR",  B0AR},
                    { "SOFUA", SOFUA },
                    { "WeBak", WeBak }
                };

            if (urlMap.ContainsKey(input))
            {
                url = urlMap[input];
            }

            if (url == "not yet set...")
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
                Console.WriteLine("Populating score..." + DateTime.Now);
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

            if (messageBuilder.Length > 0)
            {
                await chnl.SendMessageAsync(messageBuilder.ToString());
            }
            else
            {
                await chnl.SendMessageAsync("No changes.");
            }

        }

        public async Task<List<String>> GeneratePlayerList(DiscordSocketClient _client, ulong channelId, List<string> playerList)
        {
            await Task.Yield();
            // Fetch the voice channel using its ID
            var voiceChannel = _client.GetChannel(channelId) as SocketVoiceChannel;

            if (voiceChannel != null)
            {
                // Fetch the voice states of users in the voice channel
                var allusers = voiceChannel.Users;

                foreach (var user in allusers)
                {
                    var currUser = user;

                    if (currUser.VoiceState != null)
                    {
                        //playerList.Add(currUser.Id + " (" + currUser.DisplayName + ")\n");
                        playerList.Add($"{currUser.Id} ({currUser.DisplayName})\n");
                    }
                }

                // Fetch the text channel for sending the message
                ulong textChannelId = (ulong)1133615880488628344;
                ITextChannel textChannel = _client.GetChannel(textChannelId) as ITextChannel;

                if (textChannel != null)
                {
                    // Send the list of players to the text channel
                    // await textChannel.SendMessageAsync($"Connected Players: {playerListString}");
                    return playerList;
                }
                else
                {
                    Console.WriteLine("Text channel not found.");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("Voice channel not found.");
                return null;
            }
        }

        public async Task UpdatePlayerIDs(SquadronObj squadronObject)
        {
            try
            {
                // Read all lines from the file
                string[] lines = await Task.Run(() => File.ReadAllLines("PlayersToIDs.txt"));

                string currentPlayerName = null;
                ulong currentPlayerID = 0;

                foreach (string line in lines)
                {
                    // Reset player data at the start of each iteration
                    if (line.StartsWith("Name: "))
                    {
                        currentPlayerName = line.Replace("Name: ", "").Trim();
                        currentPlayerID = 0; // Reset the ID
                    }

                    if (line.StartsWith("ID: "))
                    {
                        string lineid = line.Replace("ID: ", "");
                        lineid.Trim();
                        ulong.TryParse(lineid, out ulong lineid2);
                        currentPlayerID = lineid2;

                    }

                    if (line == "-------------------------" && currentPlayerName != null && currentPlayerID != 0)
                    {
                        // Find the player in squadronObject and update the ID
                        var player = squadronObject.Players.FirstOrDefault(p => p.PlayerName == currentPlayerName);
                        if (player != null)
                        {
                            player.DiscordID = currentPlayerID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating player IDs: {ex.Message}");
            }
        }

        public async Task<SquadronObj> LoadSqd(string input)
        {
            // Implementation for the !readsqd command
            //string input = message.Content.Substring("!readsqd ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                //var chnl = message.Channel as IMessageChannel;
                string directoryPath = AppDomain.CurrentDomain.BaseDirectory;

                // Search for files in the directory with the specified squadron name in the filename
                string[] files = Directory.GetFiles(directoryPath, $"{input}_*.txt");

                if (files.Length > 0)
                {
                    // Get the most recent file based on creation time
                    string mostRecentFile = files.OrderByDescending(f => File.GetCreationTime(f)).First();

                    //await chnl.SendMessageAsync("Reading the most recent file for " + input + ": " + mostRecentFile);
                    Console.WriteLine("reading the most recent file");


                    // Populate the SquadronObj from the most recent file
                    Commands commands = new Commands();
                    SquadronObj squadronObject = commands.PopulateSquadronFromTextFile(mostRecentFile);

                    // Use the squadronObject as needed
                    //scrapeAllAndPopulate.printPlayers(chnl, squadronObject);

                    return squadronObject;

                }
                else
                {
                    Console.WriteLine("No files found for " + input + ".");
                    //chnl.SendMessageAsync("No files found for " + input + ".");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("Squadron needs to be Cadet, BofSs, or Academy.");
                //message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
                return null;
            }

        }

        public List<PlayerRatingChange> CompareSquadrons(SquadronObj oldSqd, SquadronObj newSqd)
        {
            List<PlayerRatingChange> ratingChanges = new List<PlayerRatingChange>();

            // Assuming players are uniquely identified by their names
            foreach (var oldPlayer in oldSqd.Players)
            {
                var newPlayer = newSqd.Players.FirstOrDefault(p => p.PlayerName == oldPlayer.PlayerName);

                if (newPlayer != null && oldPlayer.PersonalClanRating != newPlayer.PersonalClanRating)
                {
                    // Player's rating has changed
                    ratingChanges.Add(new PlayerRatingChange
                    {
                        PlayerName = newPlayer.PlayerName,
                        OldRating = oldPlayer.PersonalClanRating,
                        NewRating = newPlayer.PersonalClanRating
                    });
                }
            }

            return ratingChanges;
        }

        public class PlayerRatingChange
        {
            public string PlayerName { get; set; }
            public int OldRating { get; set; }
            public int NewRating { get; set; }
        }

        

    }

    }
