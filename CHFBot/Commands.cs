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
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Specialized;
using System.Threading;
using System.Security.Cryptography;

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

        public async void printPlayersOverUnder2(IMessageChannel chnl, SquadronObj sqdobj, String overUnder, int points)
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
                        // Format the DateOfEntry property as a short date string
                        string formattedDate = player.DateOfEntry.ToShortDateString();
                        sb.AppendLine($"Pts: {player.PersonalClanRating} {player.PlayerName} {formattedDate} "); // Use formattedDate{player.DateOfEntry} ");
                    }
                }
                if (overUnder == "under")
                {
                    if (player.PersonalClanRating <= points)
                    {
                        // Format the DateOfEntry property as a short date string
                        string formattedDate = player.DateOfEntry.ToShortDateString();
                        sb.AppendLine($"Pts: {player.PersonalClanRating} {player.PlayerName,-18} {formattedDate}"); // Use formattedDate
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
            string BufSsUrl = "https://warthunder.com/en/community/claninfo/Bunch%20of%20Scrubs?69";
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
            string TFedz = "https://warthunder.com/en/community/claninfo/Trooper%20Federation";
            string AFI = "https://warthunder.com/en/community/claninfo/Anthems%20For%20Insubordinates";
            string TEHb = "https://warthunder.com/en/community/claninfo/TEHb";
            string IRAN = "https://warthunder.com/en/community/claninfo/Persian%20Warriors";
            string BriSs = "https://warthunder.com/en/community/claninfo/Brigade%20of%20Scrubs";
            string EXLY  = "https://warthunder.com/en/community/claninfo/EXLY";
            string ASP1D = "https://warthunder.com/en/community/claninfo/Aspid%20Crew";
            string Nrst  = "https://warthunder.com/en/community/claninfo/North_Steel";
            string IAVRI = "https://warthunder.com/en/community/claninfo/AVANGARD%20ENTRY%20SQUADRON";
            string R6PL  = "https://warthunder.com/en/community/claninfo/Czerwone%20Szostki";
            string EPRO  = "https://warthunder.com/en/community/claninfo/EPRO-Team";
            string CLIM  = "https://warthunder.com/en/community/claninfo/Clim";
            string VaVic = "https://warthunder.com/en/community/claninfo/Vae%20Victis%20211";
            string xTHCx = "https://warthunder.com/en/community/claninfo/Try%20Hard%20Coalition";
            string ATAKD = "https://warthunder.com/en/community/claninfo/ATTACK%20the%20D%20POINT";
            string _14QID = "https://warthunder.com/en/community/claninfo/14%20Quid";
            string SCORE = "https://warthunder.com/en/community/claninfo/SCOREBOARD";
            string _0NYX = "https://warthunder.com/en/community/claninfo/0NYX";
            string Astrx = "https://warthunder.com/en/community/claninfo/Per%20Aspera%20Ad%20Astra%20-%20X";
            string BLKFT = "https://warthunder.com/en/community/claninfo/Blackfoot";
            string VCoM = "https://warthunder.com/en/community/claninfo/Valiant%20Crew%20of%20Misfits";


            string url = "not yet set...";
            SquadronObj squadronObject = new SquadronObj(input, url);

            var urlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) // Case-insensitive comparer
                {
                    { "Cadet", cadetUrl },
                    { "BofSs", BofSsUrl },
                    { "BufSs", BufSsUrl },
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
                    { "WeBak", WeBak },
                    { "TFedz", TFedz },
                    { "AFI", AFI },
                    { "TEHb", TEHb },
                    { "BriSs", BriSs },
                    { "IRAN", IRAN },
                    { "EXLY", EXLY},
                    { "Nrst", Nrst},
                    { "IAVRI", IAVRI},
                    { "R6PL", R6PL},
                    { "EPRO", EPRO},
                    { "ASP1D", ASP1D},
                    { "CLIM", CLIM},
                    { "VaVic", VaVic},
                    { "XTHCX", xTHCx},
                    { "ATAKD", ATAKD},
                    { "14QID", _14QID},
                    { "SCORE", SCORE},
                    { "0NYX", _0NYX},
                    { "Astrx", Astrx},
                    { "BLKFT", BLKFT},
                    { "VCoM", VCoM}

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

                Webscraper scraper = new Webscraper();
                sqdobj = await scraper.scrapeWebsiteAndPopulateScoreAsync(sqdobj);


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

            if (input == "Cadet" || input == "BofSs" || input == "Academy" || input == "BriSs")
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

        public string CompareContents(string currentContent, string newContent)
        {

            
            if (currentContent.Contains("\n\n"))
            {
                currentContent = currentContent.Replace("\n\n", "\n");
            }
            if (currentContent.Contains("\n\r\n"))
            {
                currentContent = currentContent.Replace("\n\r\n", "\n");
            }

            if (newContent.Contains("\n\n"))
            {
                newContent = newContent.Replace("\n\n", "\n");
            }
            if (newContent.Contains("\n\r\n"))
            {
                newContent = newContent.Replace("\n\r\n", "\n");
            }


            // Split the content strings into lines
            string[] currentLines = currentContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] newLines = newContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Check if the number of lines is the same
            if (currentLines.Length != newLines.Length)
            {
                Console.WriteLine(" output:" + currentLines.Length + " " + newLines.Length);
                return "Error: Different number of lines in current and new content.";
            }

            // Build the output string
            StringBuilder output = new StringBuilder();

            // Add the header
            output.AppendLine("#   Name   Wins  Loss  Played  Score (Change)");
            
            // Loop through each line (assuming they have the same number of lines)
            for (int i = 1; i < currentLines.Length; i++) // Skip the header line
            {
                string currentLine = currentLines[i];
                
                string newLine = newLines[i];
               
                List<string> tempList = new List<string>();

                string[] delimiters = new string[] { " ", ":", "  " };
                
                string[] currentData = currentLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string[] newData = newLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                // Check if the data points are present and in the expected format
                if (currentData.Length != 9 || newData.Length != 9)
                {
                    Console.WriteLine(" output:" + currentData.Length + " " + newData.Length);
                    return "Error: Invalid format in one or more lines.";
                }

                // Build the formatted output line
                //string formattedLine = $"{currentData[1]}  {newData[2]}  {newData[3]}  {newData[5]} {newData[7]}";
                string formattedLine = $"{newData[0].PadRight(3, ' ')} {newData[1].PadRight(5, ' ')}  {newData[2].PadLeft(4, ' ')}  {newData[4].PadLeft(4, ' ')}  {newData[5].PadRight(4, ' ')}";

                //0 is place, 1 is name, 2 is wins, 3 is an & 4 is losses, 5 is played, 6 is the word score, 7 is the score.
                //So I need 0, 1, 2, 4, 5, 7.
                // Check for changes in each data point
                bool hasChanges = false;
                if (currentData[0] != newData[0])
                {
                    hasChanges = true;
                    int currentPos = int.Parse(currentData[0]);
                    int newPos = int.Parse(newData[0]);
                    int posDifference = newPos - currentPos;
                    formattedLine += $" (Pos: {posDifference})";
                }
                if (currentData[1] != newData[1])
                {
                    hasChanges = true;
                    String currentName = currentData[1];
                    String newName = newData[1];
                    
                    String nameDifference = "xx";
                    //find out what place the squad was in....
                    //
                    //
                    for (int j = 0; j < newLines.Length; j++)
                        {
                            // Split the line by spaces
                            string[] parts = newLines[j].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            // Check if the line contains TERVE
                            if (parts.Length > 1 && parts[1].Trim(':') == currentName)
                            {
                                // Extract the position (pos) from the first part
                                string pos = parts[0];

                                // Use pos as needed
                                Console.WriteLine($"{currentName} is at {pos} in new content.");
                            nameDifference = $"{pos}";

                                // Break out of the loop since TERVE is found
                                break;
                            }
                        }

                    
                    formattedLine += $"(from {nameDifference} to {currentData[0]}" ;
                }
                if (currentData[2] != newData[2])
                {
                    hasChanges = true;
                    int currentWins = int.Parse(currentData[2]);
                    int newWins = int.Parse(newData[2]);
                    int winsDifference = newWins - currentWins;
                    formattedLine += $" (Wins: {winsDifference})";
                }

                if (currentData[4] != newData[4])
                {
                    hasChanges = true;
                    int currentLosses = int.Parse(currentData[4]);
                    int newLosses = int.Parse(newData[4]);
                    int lossesDifference = newLosses - currentLosses;
                    formattedLine += $" (Loss: {lossesDifference})";
                }

                if (currentData[5] != newData[5])
                {
                    hasChanges = true;
                    //String currentPlayed = currentPlayed.Replace("(", "");
                    // String newPlayed = newPlayed.Replace("(", "");

                    int currentPlayed = int.Parse(currentData[5].Replace("(", "").Replace("),", ""));
                    int newPlayed = int.Parse(newData[5].Replace("(", "").Replace("),", ""));
                    int playedDifference = newPlayed - currentPlayed;
                    formattedLine += $" ({playedDifference} played)";
                }


                if (currentData[8] != newData[8])
                {
                    hasChanges = true;
                    int currentScore = int.Parse(currentData[8]);
                    int newScore = int.Parse(newData[8]);
                    int scoreDifference = newScore - currentScore;
                    formattedLine += $" ({scoreDifference} pts)";
                 }

                // Add the formatted line with change note (if any)
                output.AppendLine(hasChanges ? $"{formattedLine}" : formattedLine);
            }

            return output.ToString();
        }

        public SquadronObj[] CompareContents2(string currentContent, SquadronObj[] newContent)
        {
            if (currentContent.Contains("\n\n"))
            {
                currentContent = currentContent.Replace("\n\n", "\n");
            }
            if (currentContent.Contains("\n\r\n"))
            {
                currentContent = currentContent.Replace("\n\r\n", "\n");
            }

            // Split the content strings into lines
            string[] currentLines = currentContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var squadron in newContent)
            {
                foreach (var line in currentLines)
                {
                    
                    if (line.Contains(squadron.SquadronName))
                    {

                        //Console.WriteLine(line);
                        string[] parts = line.Split(new[] { ' ',':'}, StringSplitOptions.RemoveEmptyEntries);
                        //Console.WriteLine(parts.Length);

                        if (parts.Length >= 9)
                        {
                            int currentPos = int.Parse(parts[0]);
                            int currentWins = int.Parse(parts[2]);
                            int currentLosses = int.Parse(parts[4]);
                            int currentPlayed = int.Parse(parts[5].Trim('('));
                            int currentScore = int.Parse(parts[8]);

                            // Check for changes
                            if (squadron.Pos != currentPos ||
                                squadron.Wins != currentWins ||
                                squadron.Losses != currentLosses ||
                                squadron.BattlesPlayed != currentPlayed ||
                                squadron.Score != currentScore)
                            {
                                // Annotate the changes
                                
                                squadron.Pos = squadron.Pos;
                                squadron.PosChange = int.Parse(parts[0]) - squadron.Pos; 
                                squadron.WinsChange = squadron.Wins - currentWins;
                                squadron.LossesChange = squadron.Losses - currentLosses;
                                squadron.BattlesPlayedChange = squadron.BattlesPlayed - currentPlayed;
                                squadron.ScoreChange = squadron.Score - currentScore;
                            }
                        }
                        break; // Exit the inner loop once the squadron is found
                    }
                }
            }

            return newContent;
        }

        public async Task<String> LoadStringWithMostRecentTopSquad(IMessageChannel channel)
        {
            // Dates with leading zeros
            string currentDateLeadingZeros = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterdayDateLeadingZeros = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            string twoDaysAgoDateLeadingZeros = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");

            // Dates without leading zeros
            string currentDateNoLeadingZeros = DateTime.Now.ToString("yyyy-M-d");
            string yesterdayDateNoLeadingZeros = DateTime.Now.AddDays(-1).ToString("yyyy-M-d");
            string twoDaysAgoDateNoLeadingZeros = DateTime.Now.AddDays(-2).ToString("yyyy-M-d");

            string[] possibleFilenames =
                {
                $"TopSquadTotals_{currentDateLeadingZeros}*.txt",
                $"TopSquadTotals_{yesterdayDateLeadingZeros}*.txt",
                $"TopSquadTotals_{twoDaysAgoDateLeadingZeros}*.txt",
                $"TopSquadTotals_{currentDateNoLeadingZeros}*.txt",
                $"TopSquadTotals_{yesterdayDateNoLeadingZeros}*.txt",
                $"TopSquadTotals_{twoDaysAgoDateNoLeadingZeros}*.txt"
                };

            string mostRecentFilename = null;

            foreach (var filenamePattern in possibleFilenames)
            {
                string[] matchingFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), filenamePattern);
                if (matchingFiles.Length > 0)
                {
                    mostRecentFilename = matchingFiles[matchingFiles.Length - 1]; // Get the most recent file
                    break;
                }
            }
            
            if (mostRecentFilename != null)
            {
                // File exists, read its content and perform comparison
                string currentContent = File.ReadAllText(mostRecentFilename);
                // Perform comparison with the currentContent
                return currentContent;
            }
            else
            {
                string errorMessage = "No recent files found for comparison.";
                Console.WriteLine(errorMessage);
                await channel.SendMessageAsync(errorMessage);
                return errorMessage;
            }
        }

        public async Task ProcessAltList(IMessageChannel chnl)
        {
            // 1. Get the Live Data in memory
            SquadronObj bufss = new SquadronObj("BufSs", "https://warthunder.com/en/community/clansinfo/?id=570395");
            bufss.url = "https://warthunder.com/en/community/claninfo/Bunch%20of%20Scrubs";

            Webscraper scraper = new Webscraper();
            bufss = await scraper.ScrapeWebsiteAllAndPopulateAsync(bufss);

            // 2. Load the CSV into memory using columns B, C, and D
            // We'll store a small helper class or a tuple to keep the Number and Code together
            var altData = new List<(string Number, string Name, string Code)>();

            try
            {
                string[] csvLines = File.ReadAllLines("BufSsActivityReview.csv");

                foreach (string line in csvLines)
                {
                    var parts = line.Split(',');

                    // We need up to index 3 (Column D)
                    if (parts.Length >= 4)
                    {
                        string number = parts[1].Trim(); // Column B
                        string name = parts[2].Trim();   // Column C
                        string code = parts[3].Trim();   // Column D

                        // Skip header row if it says "Player" or "Name" in Column C
                        if (name.ToLower() == "player" || name.ToLower() == "name" || string.IsNullOrEmpty(name))
                            continue;

                        altData.Add((number, name, code));
                    }
                }
            }
            catch (Exception e)
            {
                await chnl.SendMessageAsync("Error reading CSV: " + e.Message);
                return;
            }

            // 3. Match and Build the Table String
            StringBuilder sb = new StringBuilder();
            
            // Header formatting
            sb.AppendLine($"{"#".PadRight(3)} | {"Player Name".PadRight(20)} | {"Code".PadRight(6)} | {"Points"}");
            sb.AppendLine(new string('-', 45));

            foreach (var row in altData)
            {
                // Fix: Skip the "ALT NAME" or empty rows that cause the blank line at the end
                if (string.IsNullOrWhiteSpace(row.Name) || row.Name.Equals("ALT NAME", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Match against the live list (Strict match, keeping spaces and @psn)
                var matchedPlayer = bufss.Players.FirstOrDefault(p => p.PlayerName.Equals(row.Name, StringComparison.OrdinalIgnoreCase));

                string points = matchedPlayer != null ? matchedPlayer.PersonalClanRating.ToString() : "NOT FOUND";

                // Build the row: Number | Name | Code | Points
                sb.AppendLine($"{row.Number.PadRight(3)} | {row.Name.PadRight(20)} | {row.Code.PadRight(6)} | {points}");
            }
            //sb.AppendLine("```");

            // 4. Send to Discord
            await SendLongContentAsEmbedAsync(chnl, sb.ToString());
        }


        public async Task<StringBuilder> FormatAndSendComparisonResults(SquadronObj[] newContent)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("#   Name   Wins       Losses    Played Pts");

            if (newContent != null && newContent.Length > 0 && newContent[0] != null)
            {
                int maxWinsLength = 4;
                int maxLossesLength = 4;
                int maxPlayedLength = 4;
                int maxPtsLength = 5;

                // First pass: determine max column widths (including change indicators)
                foreach (var squad in newContent)
                {
                    if (squad != null)
                    {
                        int winsTotalLength = squad.Wins.ToString().Length + (squad.WinsChange != 0 ? $"(+{Math.Abs(squad.WinsChange)})".Length : 0);
                        int lossesTotalLength = squad.Losses.ToString().Length + (squad.LossesChange != 0 ? $"(+{Math.Abs(squad.LossesChange)})".Length : 0);
                        int playedTotalLength = squad.BattlesPlayed.ToString().Length + (squad.BattlesPlayedChange != 0 ? $"(+{Math.Abs(squad.BattlesPlayedChange)})".Length : 0);
                        int ptsTotalLength = squad.Score.ToString().Length + (squad.ScoreChange != 0 ? $"(+{Math.Abs(squad.ScoreChange)})".Length : 0);

                        maxWinsLength = Math.Max(maxWinsLength, winsTotalLength);
                        maxLossesLength = Math.Max(maxLossesLength, lossesTotalLength);
                        maxPlayedLength = Math.Max(maxPlayedLength, playedTotalLength);
                        maxPtsLength = Math.Max(maxPtsLength, ptsTotalLength);
                    }
                }

                // Second pass: format each line
                foreach (var squad in newContent)
                {
                    if (squad != null)
                    {
                        string paddedPos = squad.Pos.ToString().PadRight(3);
                        string paddedName = squad.SquadronName.PadRight(5);

                        string winsChangeStr = squad.WinsChange != 0 ? $"({(squad.WinsChange > 0 ? "+" : "")}{squad.WinsChange})" : "";
                        string winsWithChange = $"{squad.Wins}{winsChangeStr}".PadRight(maxWinsLength);

                        string lossesChangeStr = squad.LossesChange != 0 ? $"({squad.LossesChange})" : "";
                        string lossesWithChange = $"{squad.Losses}{lossesChangeStr}".PadRight(maxLossesLength);

                        string playedChangeStr = squad.BattlesPlayedChange != 0 ? $"({(squad.BattlesPlayedChange > 0 ? "+" : "")}{squad.BattlesPlayedChange})" : "";
                        string playedWithChange = $"{squad.BattlesPlayed}{playedChangeStr}".PadRight(maxPlayedLength);

                        string ptsChangeStr = squad.ScoreChange != 0 ? $"({(squad.ScoreChange > 0 ? "+" : "")}{squad.ScoreChange})" : "";
                        string ptsWithChange = $"{squad.Score}{ptsChangeStr}".PadRight(maxPtsLength);

                        messageBuilder.AppendLine($"{paddedPos} {paddedName}  {winsWithChange}  {lossesWithChange}  {playedWithChange}  {ptsWithChange}");
                    }
                }

                return messageBuilder;
            }
            else
            {
                messageBuilder.AppendLine("No squadron data available.");
                return messageBuilder;
            }
        }


    }

}
