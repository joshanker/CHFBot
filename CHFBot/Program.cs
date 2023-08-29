﻿using Discord;
using Discord.WebSocket;
using SquadronObjects;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using BotCommands;
using System.Collections.Generic;
using System.Text;


namespace CHFBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private readonly ulong generalChannel = 342132137064923136;
        private readonly ulong testingChannel = 1125693277295886357;
        private readonly string token = File.ReadAllText(@"token.txt");

        static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            });

            ClearMessageCache();
            _client.Log += Log;
            _client.MessageReceived += HandleCommandAsync;

            SetupTimer();
            Console.WriteLine("Timer is starting!");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private void ClearMessageCache()
        {
            PropertyInfo propertyInfo = _client.GetType().GetProperty("MessageCacheSize");
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(_client, 0);
            }
        }

        private void SetupTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer(10000 * 6 * 60); //one hour in milliseconds
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }

        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Commands quote = new Commands();
            // await quote.sendQuote(_client);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
                return;

            if (message.Channel.Name == "chf-bot-testing" || message.Channel.Name == "general" || message.Channel.Name == "esper-bot-testing")
            {
                string content = message.Content.Trim();

                if (content.StartsWith("!hello"))
                {
                    await message.Channel.SendMessageAsync("Well hi there.");
                }
                else if (content.StartsWith("!embed test"))
                {
                    var embedBuilder = new EmbedBuilder();
                    embedBuilder.Description = "blah blah";
                    await message.Channel.SendMessageAsync(embed: embedBuilder.Build());
                }
                else if (content.StartsWith("!ping"))
                {
                    await message.Channel.SendMessageAsync("Pong!");
                }
                else if (content.StartsWith("!join"))
                {
                    await HandleJoinCommand(message);
                }
                else if (content.StartsWith("!acad"))
                {
                    await HandleAcadCommand(message);
                }
                else if (content.StartsWith("!scrapesquadron "))
                {
                    await HandleScrapeSquadronCommand(message);
                }
                else if (content.StartsWith("!squadronsum "))
                {
                    await HandleSquadronSumCommand(message);
                }
                else if (content.StartsWith("!totals "))
                {
                    await HandleTotalsCommand(message);
                }
                else if (content.StartsWith("!writesqd "))
                {
                    await HandleWriteSqdCommand(message);
                }
                else if (content.StartsWith("!readsqd "))
                {
                    await HandleReadSqdCommand(message);
                }
                else if (content.StartsWith("!top20 "))
                {
                    await HandleTop20Command(message);
                }
                else if (content.StartsWith("!quote"))
                {
                    Commands getQuote = new Commands();
                    string quote = getQuote.getQuote();
                    await message.Channel.SendMessageAsync(quote);
                }
                else if (content.StartsWith("!compare "))
                {
                    await HandleCompareCommand(message);
                }
                else if (content.StartsWith("!randocommando"))
                {
                    await HandleRandoCommandoCommand(message);
                }
                else if (content.StartsWith("!commands"))
                {
                    await HandleCommandsCommand(message);
                }
                else if (content.StartsWith("!qpoints"))
                {
                    await HandleQpointsCommand(message);
                }

                else
                {
                    Console.WriteLine("No matching command detected.");
                }

            }
        }

        private async Task HandleJoinCommand(SocketMessage message)
        {
            // Implementation for the !join command
            await message.Channel.SendMessageAsync("OK, " + message.Author + ", I've got you listed on my roster!");

            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter("C:\\Roster.txt", true);

                //Write a line of text
                sw.WriteLine(message.Author + " has joined at " + DateTime.Now);
                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        private async Task HandleAcadCommand(SocketMessage message)
        {
            // Implementation for the !acad command
            Commands scrapeAllAndPopulate = new Commands();
            SquadronObj AcadObj = new SquadronObj("Academy", "https://warthunder.com/en/community/claninfo/The%20Academy");

            AcadObj = await scrapeAllAndPopulate.scrapeAllAndPopulate(AcadObj).ConfigureAwait(true);

            await message.Channel.SendMessageAsync("Squadron  Name: " + AcadObj.SquadronName + ". URL: " + AcadObj.url).ConfigureAwait(true);

            var chnl = message.Channel as IMessageChannel; // 4

            await chnl.SendMessageAsync("Squadron: " + AcadObj.SquadronName);
            await chnl.SendMessageAsync("Player Count: " + AcadObj.Players.Count);
            await chnl.SendMessageAsync("-");

            scrapeAllAndPopulate.printPlayers(chnl, AcadObj);

            //await message.Channel.SendMessageAsync("End of squadron printout.").ConfigureAwait(true);
        }

        private async Task HandleScrapeSquadronCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string squadronName = content.Substring("!scrapesquadron ".Length);

            if (squadronName == "Cadet" || squadronName == "BofSs" || squadronName == "Academy")
            {
                Commands scrapeAllAndPopulate = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = scrapeAllAndPopulate.validateSquadron(squadronName);

                squadronObject = await scrapeAllAndPopulate.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

                await message.Channel.SendMessageAsync("Squadron  Name: " + squadronObject.SquadronName + ". \rSquadron URL: " + squadronObject.url).ConfigureAwait(true);

                var chnl = message.Channel as IMessageChannel; // 4

                await message.Channel.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
                //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
                await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
                await chnl.SendMessageAsync("-");
                

                scrapeAllAndPopulate.printPlayers(chnl, squadronObject);
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private async Task HandleSquadronSumCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string input = content.Substring("!squadronsum ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                Commands scrapeAllAndPopulate = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = scrapeAllAndPopulate.validateSquadron(input);

                squadronObject = await scrapeAllAndPopulate.populateScore(squadronObject).ConfigureAwait(true);

                await message.Channel.SendMessageAsync("Squadron  Name: " + squadronObject.SquadronName + ". \rSquadron URL: " + squadronObject.url).ConfigureAwait(true);

                var chnl = message.Channel as IMessageChannel;

                
                
                await chnl.SendMessageAsync("-");

                await chnl.SendMessageAsync("Total Score: " + squadronObject.Score.ToString());
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private async Task HandleTotalsCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string input = content.Substring("!totals ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                Commands scrapeAllAndPopulate = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = scrapeAllAndPopulate.validateSquadron(input);

                squadronObject = await scrapeAllAndPopulate.populateScore(squadronObject).ConfigureAwait(true);
                squadronObject = await scrapeAllAndPopulate.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);
                var chnl = message.Channel as IMessageChannel;

                await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
                await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
                await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

                scrapeAllAndPopulate.printPlayers(chnl, squadronObject);
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private async Task HandleWriteSqdCommand(SocketMessage message)
        {
            // Implementation for the !writesqd command
            string input = message.Content.Substring("!writesqd ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                Commands scrapeAllAndPopulate = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = scrapeAllAndPopulate.validateSquadron(input);
                squadronObject = await scrapeAllAndPopulate.populateScore(squadronObject).ConfigureAwait(true);
                squadronObject = await scrapeAllAndPopulate.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);
                var chnl = message.Channel as IMessageChannel;

                string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"{input}_{dateTimeString}.txt";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                //string fileName = $"C:\\Users\\josh1\\Documents\\{input}.txt"; // Customize the file path and name as needed
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Squadron: " + squadronObject.SquadronName);
                    writer.WriteLine("Player Count: " + squadronObject.Players.Count);
                    writer.WriteLine("Score: " + squadronObject.Score.ToString());

                    foreach (Player player in squadronObject.Players)
                    {
                        writer.WriteLine($"Name: {player.PlayerName}");
                        writer.WriteLine($"Number: {player.Number}");
                        writer.WriteLine($"Personal Clan Rating: {player.PersonalClanRating}");
                        writer.WriteLine($"Activity: {player.Activity}");
                        writer.WriteLine($"Role: {player.Rank}");
                        writer.WriteLine($"Date of Entry: {player.DateOfEntry}");
                        writer.WriteLine("-------------------------");
                    }

                    await chnl.SendMessageAsync("complete");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private async Task HandleReadSqdCommand(SocketMessage message)
        {
            // Implementation for the !readsqd command
            string input = message.Content.Substring("!readsqd ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                var chnl = message.Channel as IMessageChannel;
                string directoryPath = AppDomain.CurrentDomain.BaseDirectory;

                // Search for files in the directory with the specified squadron name in the filename
                string[] files = Directory.GetFiles(directoryPath, $"{input}_*.txt");

                if (files.Length > 0)
                {

                    // Get the most recent file based on creation time
                    string mostRecentFile = files.OrderByDescending(f => File.GetCreationTime(f)).First();

                    await chnl.SendMessageAsync("Reading the most recent file for " + input + ": " + mostRecentFile);



                    // Populate the SquadronObj from the most recent file
                    Commands scrapeAllAndPopulate = new Commands();
                    SquadronObj squadronObject = scrapeAllAndPopulate.PopulateSquadronFromTextFile(mostRecentFile);

                    // Use the squadronObject as needed
                    scrapeAllAndPopulate.printPlayers(chnl, squadronObject);

                   
                }
                else
                {
                    await chnl.SendMessageAsync("No files found for " + input + ".");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private async Task HandleTop20Command(SocketMessage message)
        {
            string content = message.Content.Trim();
            string input = content.Substring("!top20 ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                Commands scrapeAllAndPopulate = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = scrapeAllAndPopulate.validateSquadron(input);

                squadronObject = await scrapeAllAndPopulate.scrapeAllAndPopulate(squadronObject);

                var chnl = message.Channel as IMessageChannel;

                if (squadronObject.Players.Count == 0)
                {
                    await chnl.SendMessageAsync("No players found for " + input + ".");
                    return;
                }

                // Sort players by score in descending order
                List<Player> top20Players = squadronObject.Players.OrderByDescending(p => p.PersonalClanRating).Take(20).ToList();

               scrapeAllAndPopulate.PrintTop20Players(chnl, squadronObject, top20Players);


            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private async Task HandleCompareCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string squadronName = content.Substring("!compare ".Length);

            if (squadronName == "Cadet" || squadronName == "BofSs" || squadronName == "Academy")
            {
                Commands commands = new Commands();

                var chnl = message.Channel as IMessageChannel;

                string[] mostRecentFiles = GetMostRecentFiles(squadronName, 2);

                if (mostRecentFiles.Length >= 2)
                {
                    await commands.CompareSquadronFiles(chnl, mostRecentFiles[0], mostRecentFiles[1]);
                }
                else
                {
                    await chnl.SendMessageAsync("There are not enough recent files to compare.");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
            }
        }

        private string[] GetMostRecentFiles(string squadronName, int count)
        {
            // Get the directory path where the files are stored
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory;

            // Search for files in the directory with the specified squadron name in the filename
            string[] files = Directory.GetFiles(directoryPath, $"{squadronName}_*.txt");

            // Get the most recent 'count' files based on creation time
            string[] mostRecentFiles = files.OrderByDescending(f => File.GetCreationTime(f)).Take(count).ToArray();

            return mostRecentFiles;
        }

        private async Task HandleRandoCommandoCommand(SocketMessage message)
        {
            string[] gameModes = { "AB", "RB" }; // Available game modes
            double[] battleRatings = GenerateBattleRatings(); // Generate the list of battle ratings
            string[] battleTypes = { "ground", "air" }; // Available battle types

            Random random = new Random();

            string selectedGameMode = gameModes[random.Next(0, gameModes.Length)]; // Randomly choose a game mode
            double selectedBattleRating = battleRatings[random.Next(0, battleRatings.Length)]; // Randomly choose a battle rating
            string selectedBattleType = battleTypes[random.Next(0, battleTypes.Length)]; // Randomly choose a battle type

            string response = $"Okay, how about: {selectedBattleRating:F1}, {selectedBattleType} {selectedGameMode}?";

            await message.Channel.SendMessageAsync(response);
        }

        private double[] GenerateBattleRatings()
        {
            List<double> battleRatings = new List<double>();

            for (double wholePart = 1.0; wholePart <= 12.0; wholePart++)
            {
                battleRatings.Add(wholePart);
                battleRatings.Add(wholePart + 0.3);
                battleRatings.Add(wholePart + 0.7);
            }

            return battleRatings.ToArray();
        }

        private async Task HandleCommandsCommand(SocketMessage message)
        {
            string content = message.Content.Trim();

            if (content.StartsWith("!commands"))
            {
                MethodInfo[] methods = typeof(Program).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.Name.StartsWith("Handle") && method.Name.EndsWith("Command") && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(SocketMessage))
                    .ToArray();

                List<string> commandList = new List<string>();

                foreach (MethodInfo method in methods)
                {
                    string methodName = method.Name;
                    string command = "!" + methodName.Substring("Handle".Length, methodName.Length - "HandleCommand".Length);
                    commandList.Add(command);
                }

                string commandsText = string.Join("\n", commandList);

                await message.Channel.SendMessageAsync("Available commands:\n" + commandsText);
            }
        }

        private async Task HandleQpointsCommand(SocketMessage message)
        {
            //Commands scrapeAllAndPopulate = new Commands();
            //SquadronObj squadronObject = new SquadronObj();


            //squadronObject = await scrapeAllAndPopulate.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);
            //700529928948678777 (Lounge)
            //1133615880488628344 (esper bot testing)
            //200594110044700675 (server ID)

            var chnl = message.Channel as IMessageChannel; // 4
            //chnl.Id = 700529928948678777;
            
            
            ulong channelId = (ulong)700529928948678777;
            IVoiceChannel voiceChannel = _client.GetChannel(channelId) as IVoiceChannel;


            GeneratePlayerListAsync(voiceChannel.Id);

            //await message.Channel.SendMessageAsync(response);
        }

        private async Task GeneratePlayerListAsync(ulong channelId)
        {
            // Fetch the voice channel using its ID
            var voiceChannel = _client.GetChannel(channelId) as SocketVoiceChannel;

            if (voiceChannel != null)
            {
                // Fetch the voice states of users in the voice channel
                var allusers = voiceChannel.Users;

                // Create a list to store member usernames
                List<string> playerList = new List<string>();

                foreach (var user in allusers)
                {
                    var currUser = user;


                    if (currUser.VoiceState != null)
                    {
                        playerList.Add(currUser.DisplayName);
                    }

                    
                }

                // Join the usernames into a single string
                string playerListString = string.Join(", ", playerList);

                // Fetch the text channel for sending the message
                ulong textChannelId = (ulong)1133615880488628344;
                ITextChannel textChannel = _client.GetChannel(textChannelId) as ITextChannel;

                if (textChannel != null)
                {
                    // Send the list of players to the text channel
                    await textChannel.SendMessageAsync($"Players in the voice channel: {playerListString}");
                }
                else
                {
                    Console.WriteLine("Text channel not found.");
                }
            }
            else
            {
                Console.WriteLine("Voice channel not found.");
            }
        }




    }


}
