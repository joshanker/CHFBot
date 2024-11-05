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

            string url = "not yet set...";
            SquadronObj squadronObject = new SquadronObj(input, url);

            var urlMap = new Dictionary<string, string>
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
                    { "IRAN", IRAN }
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

        public async Task<StringBuilder> FormatAndSendComparisonResults(SquadronObj[] newContent)
        {


            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("       Name     Wins    Losses    Played      Pts");
            if (newContent[0] != null)
            {


                foreach (var squadronObj in newContent)
                {
                    string paddedPos = squadronObj.Pos.ToString().PadRight(2, ' ');
                    string posChangeStr = squadronObj.PosChange != 0 ? $"({squadronObj.PosChange.ToString().PadLeft(2, ' ')})" : "    ";

                    string paddedName;
                    //string paddedWins = squadronObj.Wins.ToString().PadLeft(3, ' ');
                    string paddedWins = squadronObj.Wins != 0 ? squadronObj.Wins.ToString().PadLeft(3, ' ') : "   ";
                    //string paddedLosses = squadronObj.Losses.ToString().PadLeft(3, ' '); ;
                    string paddedLosses = squadronObj.Losses != 0 ? squadronObj.Losses.ToString().PadLeft(3, ' ') : "   ";

                    //string WinsChange = squadronObj.WinsChange.ToString().PadLeft(2,' ');
                    //string LossesChange = squadronObj.LossesChange.ToString().PadLeft(2, ' ');
                    //string BattlesPlayedChanged = squadronObj.BattlesPlayedChange.ToString().PadLeft(3, ' ');
                    //int ScoreChange = squadronObj.ScoreChange;

                    string WinsChange = squadronObj.WinsChange != 0 ? squadronObj.WinsChange.ToString().PadLeft(2, ' ') : " ";
                    string LossesChange = squadronObj.LossesChange != 0 ? squadronObj.LossesChange.ToString().PadLeft(2, ' ') : " ";
                    string BattlesPlayedChanged = squadronObj.BattlesPlayedChange != 0 ? squadronObj.BattlesPlayedChange.ToString().PadLeft(3, ' ') : " ";

                    string ScoreChange = squadronObj.ScoreChange != 0 ? squadronObj.ScoreChange.ToString() : " ";

                    // Include parentheses only when the corresponding value is non-zero
                    string winsChangeStr = squadronObj.WinsChange != 0 ? $"({WinsChange})" : "";
                    string lossesChangeStr = squadronObj.LossesChange != 0 ? $"({LossesChange})" : "";
                    string battlesPlayedChangedStr = squadronObj.BattlesPlayedChange != 0 ? $"({BattlesPlayedChanged})" : "";
                    string scoreChangeStr = squadronObj.ScoreChange != 0 ? $"({ScoreChange})" : "";


                    if (squadronObj.Pos < 10)
                    {

                        paddedName = squadronObj.SquadronName.PadRight(6, ' ');
                    }
                    else
                    {
                        paddedName = squadronObj.SquadronName.PadRight(6, ' ');
                    }

                    messageBuilder.AppendLine($"{paddedPos}{posChangeStr} {paddedName} {paddedWins}{winsChangeStr} & {paddedLosses}{lossesChangeStr}. {squadronObj.BattlesPlayed}{battlesPlayedChangedStr}. {squadronObj.Score}{scoreChangeStr} ");



                    //!3comparescrapemessageBuilder.AppendLine($"{paddedPos} {paddedName} {paddedWins}{winsChangeStr} & {paddedLosses}{lossesChangeStr}. ({squadronObj.BattlesPlayed}){battlesPlayedChangedStr}. {squadronObj.Score}{scoreChangeStr} ");


                    //messageBuilder.AppendLine($"{paddedPos} {paddedName} {paddedWins}({WinsChange}) & {paddedLosses}({LossesChange}). ({squadronObj.BattlesPlayed})({BattlesPlayedChanged}). {squadronObj.Score}({ScoreChange}) ");
                }



                return messageBuilder;
            }
            else
            {
                return messageBuilder;
            }

        }

    }

}
