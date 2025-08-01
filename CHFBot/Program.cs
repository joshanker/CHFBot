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
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Scraper;
using System.Globalization;
using System.Xml.Schema;
using System.CodeDom;
using System.Threading;




namespace CHFBot
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class CommandDescriptionAttribute : Attribute
    {
        public string Description { get; }

        public CommandDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    class Program
    {
        private static DiscordSocketClient _client;
        private static readonly ulong EsperBotTestingChannel = 1133615880488628344;
        private static readonly ulong sreScoreTrackingChannel = 742213810752061471;
        private static readonly ulong esperbotchannel = 1165452109513244673;
        private static readonly ulong senateChannel = 484153871510405123;
        private static readonly ulong bufssScoreTrackingChannel = 886756342210117663;
        //private readonly ulong generalChannel = 342132137064923136;
        private readonly ulong CadetTestingChannel = 1125693277295886357;
        ulong officerRoleId = 410251113955196928;
        private readonly string token = File.ReadAllText(@"token.txt");
        public bool trackVoiceUpdates = false;
        public bool minuteTimerFive = false;
        public bool bundsBotScoreTracking = false;
        public bool briSsScoreTracking = false;
        public bool quotes = false;
        public bool wlCounter = true;
        int winCounter = 0;
        int lossCounter = 0;
        int bufSsWinCounter = 0;
        int bufSsLossCounter = 0;
        public Dictionary<ulong, int> userNumbers = new Dictionary<ulong, int>(); // Stores user ID -> assigned number
        public int takeaANumberNumber = 1; // Tracks the next available number
        System.Timers.Timer hourlyTimer = new System.Timers.Timer(1000 * 60 * 60); //one hour in milliseconds
        System.Timers.Timer dailyTimer = new System.Timers.Timer(1000 * 60 * 60 * 24); //one day in milliseconds
        System.Timers.Timer midDailyTimer = new System.Timers.Timer(1000 * 60 * 60 * 24); //one day in milliseconds
        System.Timers.Timer fiveMinuteTimer = new System.Timers.Timer(1000 * 60 * 5);
        System.Timers.Timer oneMinuteTimer = new System.Timers.Timer(1000 * 60 * 1);
        int squadronTotalScore = 0;
        int squadronTotalScoreBufSs = 0;
        int squadronTotalScoreBriSs = 0;
        int endOfSessionScore = 0;
        int endOfSessionScoreBufSs = 0;
        int endOfSessionScoreBriSs = 0;

        //SquadronObj wlBaselineBofSs = new SquadronObj();
        //SquadronObj wlBaselineBufSs = new SquadronObj();

        int startOfSessionWins = 0;
        int startOfSessionLosses = 0;
        int midSessionWinsCounter = 0;
        int midSessionLossesCounter = 0;
        int lastRunsWinsCumulativeCounter = 0;
        int lastRunsLossesCumulativeCounter = 0;
        int startOfSessionPoints = 0;
        int sessionScoreDelta = 0;

        int StartOfSessionWinsBufSs = 0;
        int StartOfSessionLossesBufSs = 0;
        int midSessionWinsCounterBufSs = 0;
        int midSessionLossesCounterBufSs = 0;
        int lastRunsWinsCumulativeCounterBufSs = 0;
        int lastRunsLossesCumulativeCounterBufSs = 0;
        int startOfSessionPointsBufSs = 0;
        int sessionScoreDeltaBufSs = 0;

        int StartOfSessionWinsBriSs = 0;
        int StartOfSessionLossesBriSs = 0;
        int midSessionWinsCounterBriSs = 0;
        int midSessionLossesCounterBriSs = 0;
        int lastRunsWinsCumulativeCounterBriSs = 0;
        int lastRunsLossesCumulativeCounterBriSs = 0;
        int startOfSessionPointsBriSs = 0;
        int sessionScoreDeltaBriSs = 0;

        private Dictionary<string, bool> featureToggles = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            { "quotes", false },
            { "minuteTimerFive", false },
            { "bundsBotScoreTracking", false },
            { "briSsScoreTracking", false }
        };



        ////////////// On startup, let's see if we can pull the score.....
        ////////////////
        ////////////////
        ////////////////
        ////////////////




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
            _client.MessageReceived += HandleCommandsAsync;
            _client.UserVoiceStateUpdated += HandleVoiceStateUpdated;

            SetupTimer();

            startupMessages();

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

        private async void startupMessages()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            Console.WriteLine("Bot started up.");
            ITextChannel chnl = _client.GetChannel(EsperBotTestingChannel) as ITextChannel;
            ITextChannel srescoretrackingchnl = _client.GetChannel(sreScoreTrackingChannel) as ITextChannel;
            ITextChannel esperbotchnl = _client.GetChannel(esperbotchannel) as ITextChannel;

            //ScoreExtractor extractor = new ScoreExtractor();
            //int scoreOfBofSs = extractor.ExtractScoreOfBofSs();
            //Console.WriteLine($"Score of BofSs: {scoreOfBofSs}");
            //endOfSessionScore = scoreOfBofSs;


            //ProcessSquadron5mScoreChange("BofSs");
            //ProcessSquadron5mScoreChange("BufSs");
            ProcessSquadron1mScoreChanges();


            Commands commands = new Commands();
            SquadronObj sqdObj = new SquadronObj();
            sqdObj.url = "https://warthunder.com/en/community/claninfo/Band%20Of%20Scrubs";
            await commands.populateScore(sqdObj);
            squadronTotalScore = sqdObj.Score;
            endOfSessionScore = sqdObj.Score;

            SquadronObj sqdObj2 = new SquadronObj();
            sqdObj2.url = "https://warthunder.com/en/community/claninfo/Bunch%20of%20Scrubs?69";
            await commands.populateScore(sqdObj2);
            squadronTotalScoreBufSs = sqdObj2.Score;
            endOfSessionScoreBufSs = sqdObj2.Score;

            SquadronObj sqdObj3 = new SquadronObj();
            sqdObj3.url = "https://warthunder.com/en/community/claninfo/Brigade%20of%20Scrubs";
            await commands.populateScore(sqdObj3);
            squadronTotalScoreBriSs = sqdObj3.Score;
            endOfSessionScoreBriSs = sqdObj3.Score;


            HandleCheckCommand("!check BofSs", chnl);
            HandleCheckCommand("!check BufSs", chnl);
            HandleCheckCommand("!check BriSs", chnl);
            HandleCheckCommand("!check BofSs", esperbotchnl);
            HandleCheckCommand("!check BufSs", esperbotchnl);
            HandleCheckCommand("!check BriSs", esperbotchnl);

            int scoreOfBofSs = sqdObj.Score;
            int scoreOfBufSs = sqdObj2.Score;
            int scoreOfBriSs = sqdObj3.Score;

            await chnl.SendMessageAsync("EsperBot online!. Quotes: " + quotes + ". " + "Voice channel tracking: " + trackVoiceUpdates + ". " + "5m timer: " + minuteTimerFive + ". BundsBot score tracking: " + bundsBotScoreTracking + ". BriSs score tracking: " + briSsScoreTracking + ". Setting last recorded score to " + scoreOfBofSs + ". SRE score set to 0-0.  Use !help for a command list.");




        }

        ////////////////////////////////////////////////////////
        //Timers
        ////////////////////////////////////////////////////////

        private void SetupTimer()
        {

            Console.WriteLine("starting the timer.");

            //hourlyTimer.Elapsed += OnHourlyEvent;
            hourlyTimer.AutoReset = false;
            dailyTimer.AutoReset = false;
            midDailyTimer.AutoReset = false;

            // Calculate the time until 4:30 AM tomorrow
            DateTime now = DateTime.Now;
            DateTime targetTime = new DateTime(now.Year, now.Month, now.Day, 3, 30, 0);

            if (now > targetTime)
            {
                // If it's already past 4:30 AM, schedule for the next day
                targetTime = targetTime.AddDays(1);
            }
            double interval = (targetTime - now).TotalMilliseconds;
            dailyTimer.Interval = interval;
            dailyTimer.Elapsed += OnDailyEvent;

            // Calculate the time until 19:00 tomorrow
            DateTime midNow = DateTime.Now;
            DateTime midTargetTime = new DateTime(midNow.Year, midNow.Month, midNow.Day, 18, 30, 0);

            if (midNow > midTargetTime)
            {
                // If it's already past 19:00, schedule for the next day
                midTargetTime = midTargetTime.AddDays(1);
            }

            double midInterval = (midTargetTime - midNow).TotalMilliseconds;
            midDailyTimer.Interval = midInterval;
            midDailyTimer.Elapsed += OnMidDailyEvent;

            // Calculate the time until next on-the-hour
            DateTime hourlyNow = DateTime.Now;
            //DateTime nextHourly = new DateTime(hourlyNow.Year, hourlyNow.Month, hourlyNow.Day, hourlyNow.Hour + 1, 0, 0);

            DateTime nextHourly;

            if (hourlyNow.Hour == 23)
            {
                // If the current hour is 23 (11 PM), check if it's the last day of the month
                if (hourlyNow.Day == DateTime.DaysInMonth(hourlyNow.Year, hourlyNow.Month))
                {
                    // If it's the last day of the month, set the next hourly time to 00 (12 AM) of the first day of the next month
                    nextHourly = new DateTime(hourlyNow.Year, hourlyNow.Month + 1, 1, 0, 0, 0);
                }
                else
                {
                    // Otherwise, set the next hourly time to 00 (12 AM) of the next day
                    nextHourly = new DateTime(hourlyNow.Year, hourlyNow.Month, hourlyNow.Day + 1, 0, 0, 0);
                }
            }
            else
            {
                // Otherwise, increment the current hour by 1
                nextHourly = hourlyNow.AddHours(1);
            }


            if (midNow > nextHourly)
            {
                // If it's already past 19:00, schedule for the next day
                nextHourly = nextHourly.AddHours(1);
            }

            double hourlyInterval = (nextHourly - hourlyNow).TotalMilliseconds;
            hourlyTimer.Interval = hourlyInterval;
            hourlyTimer.Elapsed += OnHourlyEvent;

            fiveMinuteTimer.Elapsed += OnFiveMinuteEvent;
            fiveMinuteTimer.AutoReset = true; // Ensure it automatically resets

            oneMinuteTimer.Elapsed += OnOneMinuteEvent;
            oneMinuteTimer.AutoReset = true; // Ensure it automatically resets

            hourlyTimer.Start();
            dailyTimer.Start();
            midDailyTimer.Start();
            fiveMinuteTimer.Start();
            oneMinuteTimer.Start();


            
        }
        private async void OnHourlyEvent(object source, ElapsedEventArgs e)
        {
            Commands command = new Commands();

            if (quotes == true)
            {
                string quote = command.getQuote();
                IMessageChannel chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
                await chnl.SendMessageAsync(quote);
            }

            hourlyTimer.Interval = 60 * 60 * 1000; // One hour in milliseconds
            hourlyTimer.Start();
        }
        private async void OnDailyEvent(object source, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now.AddDays(-1);
       

            ResetWLSessionVariables();


            // Create a date and time prefix
            string dateTimePrefix = $"{now.Year}-{now.Month}-{now.Day}-US Session:{now.Hour}:{now.Minute}:{now.Second}";
            await executeTimer(dateTimePrefix);
            dailyTimer.Interval = 24 * 60 * 60 * 1000; // 24 hours in milliseconds
            dailyTimer.Start();
            //pointsCheckBriSs();
            
        }
        private async void OnMidDailyEvent(object source, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;

            ResetWLSessionVariables();

            // Create a date and time prefix
            string dateTimePrefix = $"{now.Year}-{now.Month}-{now.Day}-EU Session:{now.Hour}:{now.Minute}:{now.Second}";
            await executeTimer(dateTimePrefix);
            midDailyTimer.Interval = 24 * 60 * 60 * 1000; // 24 hours in milliseconds
            midDailyTimer.Start();
            //pointsCheckBriSs();
        }
        private async void OnFiveMinuteEvent(object source, ElapsedEventArgs e)
        {
            if (minuteTimerFive == true)
            {
                ProcessIndivPointsChange();
            }

            if (wlCounter == true)
            {
                //ProcessSquadron5mScoreChange("BofSs");
                //ProcessSquadron5mScoreChange("BufSs");
                //ProcessSquadron1mScoreChanges();
            }


        }
        private async void OnOneMinuteEvent(object source, ElapsedEventArgs e)
        {

            if (wlCounter == true)
            {
                //ProcessSquadron5mScoreChange("BofSs");
                //ProcessSquadron5mScoreChange("BufSs");

               ProcessSquadron1mScoreChanges();
                              

            }


        }
        private async Task executeTimer(String prefix)
        {
            Commands commands = new Commands();

            userNumbers.Clear();
            takeaANumberNumber = 1; // Tracks the next available number



        SquadronObj sqdObj = new SquadronObj
            {   url = "https://warthunder.com/en/community/claninfo/Band%20Of%20Scrubs",
                SquadronName = "BofSs"};

            await commands.populateScore(sqdObj);
            squadronTotalScore = sqdObj.Score;

            SquadronObj sqdObjBufSs = new SquadronObj
            {url = "https://warthunder.com/en/community/claninfo/Bunch%20of%20Scrubs",
                SquadronName = "BufSs"};

            await commands.populateScore(sqdObjBufSs);
            squadronTotalScoreBufSs = sqdObjBufSs.Score;

            SquadronObj sqdObjBriSs = new SquadronObj
            {url = "https://warthunder.com/en/community/claninfo/Brigade%20of%20Scrubs",
                SquadronName = "BriSs"};

            await commands.populateScore(sqdObjBriSs);
            squadronTotalScoreBriSs = sqdObjBriSs.Score;

            IMessageChannel chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
            ITextChannel esperbotchnl = _client.GetChannel(esperbotchannel) as ITextChannel;
            IMessageChannel bufsssrescoretrackingchl = _client.GetChannel(esperbotchannel) as IMessageChannel;

            await chnl.SendMessageAsync("Writing totals to file.");
            await esperbotchnl.SendMessageAsync("Writing totals to file.");

            //Let's also write the newer style of info........
            await WriteCheck("!check bofss");
            await WriteCheck("!check bufss");
            await WriteCheck("!check briss");

            // Create a file name with the date and time prefix
            string fileName = "SREWinLossRecords.txt";
            string fileNameBufSs = "SREWinLossRecordsBufSs.txt";
            string fileNameBriSs = "SREWinLossRecordsBriSs.txt";

            // Check if the file exists
            if (!File.Exists(fileName))
            {
                // If the file does not exist, create it
                using (File.Create(fileName)) { };
            }
            // Check if the file exists
            if (!File.Exists(fileNameBufSs))
            {
                // If the file does not exist, create it
                using (File.Create(fileNameBufSs)) { };
            }
            if (!File.Exists(fileNameBriSs))
            {
                // If the file does not exist, create it
                using (File.Create(fileNameBriSs)) { };
            }

            String currentContent = await commands.LoadStringWithMostRecentTopSquad(chnl);
            SquadronObj[] newcontent = await Webscraper.TestScrape2();
            SquadronObj[] comparisonResults = commands.CompareContents2(currentContent, newcontent);

            var lastWinCounter = 0;
            var lastLossCounter = 0;
            var lastBufSsWinCounter = 0;
            var lastBufSsLossCounter = 0;
            var lastBriSsWinCounter = 0;
            var lastBriSsLossCounter = 0;

            foreach (var squadron in comparisonResults)
            {
                //Console.WriteLine(squadron.SquadronName); 
                if (squadron.SquadronName == "BofSs")
                {
                    lastWinCounter = squadron.WinsChange;
                    lastLossCounter = squadron.LossesChange;
                }
                else if (squadron.SquadronName == "BufSs")
                {
                    lastBufSsWinCounter = squadron.WinsChange;
                    lastBufSsLossCounter = squadron.LossesChange;
                }
                else if (squadron.SquadronName == "BriSs")
                {
                    lastBriSsWinCounter = squadron.WinsChange;
                    lastBriSsLossCounter = squadron.LossesChange;
                }
            }


            try
            {
                (int[] bufssRead2, int[] bufssRead1) = ReadCheck("BufSs");
                lastBufSsWinCounter = bufssRead1[1] - bufssRead2[1];
                lastBufSsLossCounter = bufssRead1[2] - bufssRead2[2];
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error message
                Console.WriteLine("failed on the try/catch for bufssread and ReadCheck");
            }

            try
            {
                (int[] Read2, int[] Read1) = ReadCheck("BofSs");
                lastWinCounter = Read1[1] - Read2[1];
                lastLossCounter = Read1[2] - Read2[2];
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error message
                Console.WriteLine("failed on the try/catch for read and ReadCheck");
            }

            try
            {
                (int[] brissRead2, int[] brissRead1) = ReadCheck("BriSs");
                lastBriSsWinCounter = brissRead1[1] - brissRead2[1];
                lastBriSsLossCounter = brissRead1[2] - brissRead2[2];
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error message
                Console.WriteLine("failed on the try/catch for brissread and ReadCheck");
            }


            // Open the file for writing
            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                // Write the win and loss counters to the file
                writer.WriteLine($"{prefix}: Wins: {lastWinCounter}, Losses: {lastLossCounter}, Total Score: {squadronTotalScore}");
            }
            using (StreamWriter writerBufSs = new StreamWriter(fileNameBufSs, true))
            {
                // Write the win and loss counters to the file
                writerBufSs.WriteLine($"{prefix}: Wins: {lastBufSsWinCounter}, Losses: {lastBufSsLossCounter}, Total Score: {squadronTotalScoreBufSs}");
            }
            using (StreamWriter writerBriSs = new StreamWriter(fileNameBriSs, true))
            {
                // Write the win and loss counters to the file
                writerBriSs.WriteLine($"{prefix}: Wins: {lastBriSsWinCounter}, Losses: {lastBriSsLossCounter}, Total Score: {squadronTotalScoreBriSs}");
            }


            await HandleCompareScrapeCommand(esperbotchnl);
            await HandleCompareScrapeCommand(chnl);



            ////////////////////////////////
            ///let's do that again for TopSquadTotals.txt
            ////////////////////////////////


            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string topSquadFileName = prefix.Replace(":", "_").TrimEnd();
            topSquadFileName = $"TopSquadTotals_{topSquadFileName}.txt";

            // Check if the file exists
            if (!File.Exists(topSquadFileName))
            {
                // If the file does not exist, create it
                using (File.Create(topSquadFileName)) { };
            }

            string content = await Webscraper.TestScrape(); // Call the TestScrape method
            const int maxEmbedLength = 4096;
            const int maxChunkLength = 2000;
            if (content.Length <= maxEmbedLength)
            {
                // If the content fits within the limit, send it as a single embedded message
                //await message.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription($"```{content}```").Build());
                // Open the file for writing
                using (StreamWriter topSquadWriter = new StreamWriter(topSquadFileName, true))
                {
                    // Write the top squadron totals to the file
                    topSquadWriter.WriteLine($"{prefix}: {content}");
                }
            }
            else
            {
                Console.WriteLine("content.length is greater than or equal to maxEmbedLength.");
            }



            ResetWLSessionVariables();




            //esperbot testing channel:
            await chnl.SendMessageAsync("BofSs Win/Loss: (" + lastWinCounter + "-" + lastLossCounter + "). Total squadron score: " + endOfSessionScore + " -> " + squadronTotalScore + " (+" + (squadronTotalScore - endOfSessionScore).ToString() + ").");

            await chnl.SendMessageAsync("BufSs: Win/Loss: (" + lastBufSsWinCounter + "-" + lastBufSsLossCounter + "). Total squadron score: " + endOfSessionScoreBufSs + " -> " + squadronTotalScoreBufSs + " (+" + (squadronTotalScoreBufSs - endOfSessionScoreBufSs).ToString() + ").");
            
            await chnl.SendMessageAsync("BriSs: Win/Loss: (" + lastBriSsWinCounter + "-" + lastBriSsLossCounter + "). Total squadron score: " + endOfSessionScoreBriSs + " -> " + squadronTotalScoreBriSs + " (+" + (squadronTotalScoreBriSs - endOfSessionScoreBriSs).ToString() + ").");

            await esperbotchnl.SendMessageAsync("BofSs Win/Loss: (" + lastWinCounter + "-" + lastLossCounter + "). Total squadron score: " + endOfSessionScore + " -> " + squadronTotalScore + " (+" + (squadronTotalScore - endOfSessionScore).ToString() + ").");

            await esperbotchnl.SendMessageAsync("BufSs: Win/Loss: (" + lastBufSsWinCounter + "-" + lastBufSsLossCounter + "). Total squadron score: " + endOfSessionScoreBufSs + " -> " + squadronTotalScoreBufSs + " (+" + (squadronTotalScoreBufSs - endOfSessionScoreBufSs).ToString() + ").");
            await esperbotchnl.SendMessageAsync("BriSs: Win/Loss: (" + lastBriSsWinCounter + "-" + lastBriSsLossCounter + "). Total squadron score: " + endOfSessionScoreBriSs + " -> " + squadronTotalScoreBriSs + " (+" + (squadronTotalScoreBriSs - endOfSessionScoreBriSs).ToString() + ").");


            endOfSessionScore = sqdObj.Score;
            endOfSessionScoreBufSs = sqdObjBufSs.Score;
            endOfSessionScoreBriSs = sqdObjBriSs.Score;

            await HandleCheckCommand("!check BofSs", chnl);
            await HandleCheckCommand("!check BufSs", chnl);
            await HandleCheckCommand("!check BriSs", chnl);

            await HandleCheckCommand("!check BofSs", esperbotchnl);
            await HandleCheckCommand("!check BufSs", esperbotchnl);
            await HandleCheckCommand("!check BriSs", esperbotchnl);

            //HandleCheckCommand("!check BofSs", esperbotchnl);
            //HandleCheckCommand("!check BufSs", esperbotchnl);

            /////////////////////////////////
            //checks
            /////////////////////////////////
            //StringBuilder checkbofss = await ActivateCheckLoadProcess("bofss");
            //await chnl.SendMessageAsync($"```{checkbofss.ToString()}```");

            //StringBuilder checkbufss = await ActivateCheckLoadProcess("bufss");
            //await chnl.SendMessageAsync($"```{checkbufss.ToString()}```");


        }

        ////////////////////////////////////////////////////////
        //Voice States
        ////////////////////////////////////////////////////////
        private async Task HandleVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var guild = (user as SocketGuildUser)?.Guild; // Get the guild associated with the user

            if (trackVoiceUpdates == false)
            {
                return;
            }

            // Check if the user has joined a voice channel
            if (oldState.VoiceChannel == null && newState.VoiceChannel != null)
            {
                // User has joined a voice channel
                var textChannel = guild?.TextChannels.FirstOrDefault(x => x.Name == "esper-bot-testing");
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync($"{guild.GetUser(user.Id).Nickname} ({user.Username}) ({user.Id}) has connected to the Discord and joined {newState.VoiceChannel.Name} ({newState.VoiceChannel.Id}) at {DateTime.Now}");
                }
            }
            // Check if the user has left a voice channel
            else if (oldState.VoiceChannel != null && newState.VoiceChannel == null)
            {
                // User has left a voice channel
                var textChannel = guild?.TextChannels.FirstOrDefault(x => x.Name == "esper-bot-testing");
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync($"{guild.GetUser(user.Id).Nickname} ({user.Username})({user.Id}) has signed off from {oldState.VoiceChannel.Name} ({oldState.VoiceChannel.Id}) at {DateTime.Now}");
                }
            }
            // Check if the user has moved between voice channels
            else if (oldState.VoiceChannel != newState.VoiceChannel)
            {
                // User has moved between voice channels
                var textChannel = guild?.TextChannels.FirstOrDefault(x => x.Name == "esper-bot-testing");
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync($"{guild.GetUser(user.Id).Nickname} ({user.Username}) ({user.Id}) has moved from {oldState.VoiceChannel.Name} ({newState.VoiceChannel.Id}) to {newState.VoiceChannel.Name} ({newState.VoiceChannel.Id}) at {DateTime.Now}");
                }
            }
        }

        ////////////////////////////////////////////////////////
        //Commands
        ////////////////////////////////////////////////////////
        private async Task HandleCommandsAsync(SocketMessage message)
        {

            string content = message.Content.Trim();

            if (message.Channel.Name == "sre-score-tracking" && bundsBotScoreTracking)
            {
                await HandleSreScoreTrackingMessage(message);
            }

            if (message.Channel.Name == "🔍bufss-score-tracking" && bundsBotScoreTracking)
            {
                await HandleBufSsSreScoreTrackingMessage(message);
            }


            if (message.Author.IsBot)
                return;

            if (message.Channel.Name == "chf-bot-testing" || message.Channel.Name == "esper-bot-testing" || message.Channel.Name == "esperbot")
            {
                content = content.ToLower();

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
                else if (content.StartsWith("!top20"))
                {
                    await HandleTop20NoArgCommand(message);
                }
                else if (content.StartsWith("!quote"))
                {
                    if (content == "!quote")
                    {
                        Commands command = new Commands();
                        string quote = command.getQuote();
                        await message.Channel.SendMessageAsync(quote);
                    }

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
                //else if (content.StartsWith("!3qpoints"))
                //{
                //    await Handle3QpointsCommand(message);
                //}
                else if (content.StartsWith("!help"))
                {
                    await HandleCommandsCommand(message);
                }
                else if (content.StartsWith("!trackvoiceupdates"))
                {
                    await HandleTrackVoiceUpdatesCommand(message);
                }
                else if (content.StartsWith("!directbotcommands"))
                {
                    await HandleDirectBotCommandsCommand(message);
                }
                else if (content.StartsWith("!record"))
                {
                    await HandleRecordCommand(message);
                }
                else if (content.StartsWith("!2record"))
                {
                    await Handle2RecordCommand(message);
                }
                else if (content.StartsWith("!listplayers"))
                {
                    await HandleListplayersCommand(message);
                }
                else if (content.StartsWith("!dmtest"))
                {
                    await SecretdmtestCommand(message);
                }
                else if (content.StartsWith("!testcmd"))
                {
                    await SecrettestcmdCommand(message);
                }
                else if (content.StartsWith("!2testcmd"))
                {
                    await Secret2testcmdCommand(message);
                }
                else if (content.StartsWith("!3testcmd"))
                {
                    await Secret3testcmdCommand(message);
                }
                else if (content.StartsWith("!4testcmd"))
                {
                    await Secret4testcmdCommand(message);
                }
                else if (content.StartsWith("!5testcmd"))
                {
                    await Secret5testcmdCommand(message);
                }
                else if (content.StartsWith("!recent50"))
                {
                    await SecretRecent50Command(message);
                }
                else if (content.StartsWith("!setbr"))
                {
                    await HandleSetBRCommand(message);
                }
                else if (content.StartsWith("!eos"))
                {
                    await HandleEOSCommand(message);
                }
                //else if (content.StartsWith("!2listplayers"))
                //{
                //    await Handle2ListplayersCommand(message);
                //}
                //else if (content.StartsWith("!3listplayers"))
                //{
                //    await Handle3ListplayersCommand(message);
                //}
                //else if (content.StartsWith("!4listplayers"))
                //{
                //    await Handle4ListplayersCommand(message);
                //}
                //else if (content.StartsWith("!5listplayers"))
                //{
                //    await Handle5ListplayersCommand(message);
                //}
                else if (content.StartsWith("!lastten"))
                {
                    await HandleLastTenCommand(message);
                }
                else if (content.StartsWith("!setwinloss"))
                {
                    await HandleSetWinLossCom(message);
                }
                else if (content.StartsWith("!2setwinloss"))
                {
                    await Handle2SetWinLossCom(message);
                }
                else if (content.StartsWith("!setstartingscore"))
                {
                    await HandleSetStartingScoreCom(message);
                }
                else if (content.StartsWith("!2setstartingscore"))
                {
                    await Handle2SetStartingScoreCom(message);
                }
                else if (content.StartsWith("!listalts"))
                {
                    await HandleListAltsCommand(message);
                }
                else if (content.StartsWith("!resetsessionwlvariables"))
                {
                    await HandleresetsessionwlvariablesCom(message);
                }
                //else if (content.StartsWith("!testscrape"))
                //{
                //    await HandleTestScrapeCommand(message);
                //}
                else if (content.StartsWith("!scrape"))
                {
                    await HandleScrapeCommand(message);
                }
                else if (content.StartsWith("!comparescrape"))
                {
                    await HandleCompareScrapeCommand(message.Channel);
                }
                else if (content.StartsWith("!squadrontotalscore"))
                {
                    await HandleSquadronTotalScoreCom(message);
                }
                else if (content.StartsWith("!2squadrontotalscore"))
                {
                    await Handle2SquadronTotalScoreCom(message);
                }
                else if (content.StartsWith("!check"))
                {
                    await HandleCheckCommand(message);
                }
                else if (content.StartsWith("!altvehicles"))
                {
                    await HandleAltVehiclesCommand(message);
                }
                else if (content.StartsWith("!takeanumber"))
                {
                    await HandleTakeANumberCommand(message);
                }
                else if (content.StartsWith("!shownumbers"))
                {
                    await HandleShowNumbersCommand(message);
                }
                else if (content.StartsWith("!turn"))
                {
                    await HandleTurnCommand(message);
                }
                else if (content.StartsWith("!settings"))
                {
                    await HandleSettingsCommand(message);
                }
                else if (content.StartsWith("!2executetimer"))
                {
                    DateTime now = DateTime.Now.AddDays(-1);
                    string dateTimePrefix = $"{now.Year}-{now.Month}-{now.Day}-US Session:{now.Hour}:{now.Minute}:{now.Second}";
                    await executeTimer(dateTimePrefix);
                }
                else
                {
                    Console.WriteLine($"No matching command detected: {message}");
                }

            }
        }

        ////////////////////////////////////////////////////////
        //Handlers
        ////////////////////////////////////////////////////////
        private async Task HandleSreScoreTrackingMessage(SocketMessage message)
        {
            if (message.Embeds.Any())
            {
                var chnl = _client.GetChannel(esperbotchannel) as IMessageChannel;

                //message.Channel.SendMessageAsync($"OK, triggering Embeds");
                //Console.WriteLine("embed detected!");
                //chnl.SendMessageAsync("I have detected an Embed.");

                var embed2 = message.Embeds.First();
                string title = embed2.Title;
                string desc = embed2.Description;
                //await chnl.SendMessageAsync($"Description: {embed2.Description}");
                // Loop through each embed in the message
                foreach (var embed in message.Embeds)
                {
                    // Check if the embed has a title
                    if (!string.IsNullOrEmpty(embed.Title))
                    {
                        // Check if the title contains specific text
                        if (embed.Title.Contains("Squadron gained"))
                        {
                            winCounter++;
                            //await message.Channel.SendMessageAsync("I have detected a win. This makes us " + winCounter + " and " + lossCounter + ".");
                            await chnl.SendMessageAsync("I have detected a win. This makes us " + winCounter + " and " + lossCounter + ".");
                            //var chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
                            //chnl.SendMessageAsync($"Description: {embed2.Description}");
                            //chnl.SendMessageAsync("success - We won a game.);
                        }
                        else if (embed.Title.Contains("Squadron lost"))
                        {
                            lossCounter++;
                            //await message.Channel.SendMessageAsync("I have detected a loss. This makes us " + winCounter + " and " + lossCounter);
                            await chnl.SendMessageAsync("I have detected a loss. This makes us " + winCounter + " and " + lossCounter);
                            //var chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
                            //Console.WriteLine($"Loss Detected.");
                            //chnl.SendMessageAsync("success - We lost a game. This makes us \" + winCounter + \"and \" + lossCounter");

                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No matching embeds detected in sre-score-tracking.");
            }
        }

        private async Task HandleBufSsSreScoreTrackingMessage(SocketMessage message)
        {
            if (message.Embeds.Any())
            {
                var chnl = _client.GetChannel(esperbotchannel) as IMessageChannel;

                //message.Channel.SendMessageAsync($"OK, triggering Embeds");
                Console.WriteLine("embed detected!");
                //chnl.SendMessageAsync("I have detected an Embed.");

                var embed2 = message.Embeds.First();
                string title = embed2.Title;
                string desc = embed2.Description;
                //await chnl.SendMessageAsync($"Description: {embed2.Description}");
                // Loop through each embed in the message
                foreach (var embed in message.Embeds)
                {
                    // Check if the embed has a title
                    if (!string.IsNullOrEmpty(embed.Title))
                    {
                        // Check if the title contains specific text
                        if (embed.Title.Contains("Squadron gained"))
                        {
                            bufSsWinCounter++;
                            //await message.Channel.SendMessageAsync("I have detected a win. This makes us " + winCounter + " and " + lossCounter + ".");
                            await chnl.SendMessageAsync("BufSs: I have detected a win. This makes us " + bufSsWinCounter + " and " + bufSsLossCounter + ".");
                            //var chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
                            //chnl.SendMessageAsync($"Description: {embed2.Description}");
                            //chnl.SendMessageAsync("success - We won a game.);
                        }
                        else if (embed.Title.Contains("Squadron lost"))
                        {
                            bufSsLossCounter++;
                            //await message.Channel.SendMessageAsync("I have detected a loss. This makes us " + winCounter + " and " + lossCounter);
                            await chnl.SendMessageAsync("BufSs: I have detected a loss. This makes us " + bufSsWinCounter + " and " + bufSsLossCounter);
                            //var chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
                            //Console.WriteLine($"Loss Detected.");
                            //chnl.SendMessageAsync("success - We lost a game. This makes us \" + winCounter + \"and \" + lossCounter");

                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No matching embeds detected in bufss-score-tracking.");
            }
        }


        //[CommandDescription("Currently unused")]
        //private async Task HandleJoinCommand(SocketMessage message)
        //{
        //    // Implementation for the !join command
        //    await message.Channel.SendMessageAsync("OK, " + message.Author + ", I've got you listed on my roster!");

        //    try
        //    {
        //        //Pass the filepath and filename to the StreamWriter Constructor
        //        StreamWriter sw = new StreamWriter("C:\\Roster.txt", true);

        //        //Write a line of text
        //        sw.WriteLine(message.Author + " has joined at " + DateTime.Now);
        //        //Close the file
        //        sw.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Exception: " + e.Message);
        //    }
        //    finally
        //    {
        //        Console.WriteLine("Executing finally block.");
        //    }
        //}

        //private async Task HandleAcadCommand(SocketMessage message)
        //{
        //    // Implementation for the !acad command
        //    Commands scrapeAllAndPopulate = new Commands();
        //    SquadronObj AcadObj = new SquadronObj("Academy", "https://warthunder.com/en/community/claninfo/The%20Academy");

        //    AcadObj = await scrapeAllAndPopulate.scrapeAllAndPopulate(AcadObj).ConfigureAwait(true);

        //    await message.Channel.SendMessageAsync("Squadron  Name: " + AcadObj.SquadronName + ". URL: " + AcadObj.url).ConfigureAwait(true);

        //    var chnl = message.Channel as IMessageChannel; // 4

        //    await chnl.SendMessageAsync("Squadron: " + AcadObj.SquadronName);
        //    await chnl.SendMessageAsync("Player Count: " + AcadObj.Players.Count);
        //    await chnl.SendMessageAsync("-");

        //    scrapeAllAndPopulate.printPlayers(chnl, AcadObj);

        //    //await message.Channel.SendMessageAsync("End of squadron printout.").ConfigureAwait(true);
        //}

        //[CommandDescription("This might be the same as !totals... !scrapesquadron BofSs gives link, name, count, and each players' score. Doesn't give total score.")]
        //private async Task HandleScrapeSquadronCommand(SocketMessage message)
        //{
        //    string content = message.Content.Trim();
        //    string squadronName = content.Substring("!scrapesquadron ".Length);

        //    if (squadronName == "Cadet" || squadronName == "BofSs" || squadronName == "Academy")
        //    {
        //        Commands scrapeAllAndPopulate = new Commands();
        //        SquadronObj squadronObject = new SquadronObj();

        //        squadronObject = scrapeAllAndPopulate.validateSquadron(squadronName);

        //        squadronObject = await scrapeAllAndPopulate.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

        //        await message.Channel.SendMessageAsync("Squadron  Name: " + squadronObject.SquadronName + ". \rSquadron URL: " + squadronObject.url).ConfigureAwait(true);

        //        var chnl = message.Channel as IMessageChannel; // 4

        //        await message.Channel.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
        //        //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
        //        await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
        //        await chnl.SendMessageAsync("-");


        //        scrapeAllAndPopulate.printPlayers(chnl, squadronObject);
        //    }
        //    else
        //    {
        //        await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
        //    }
        //}

        //[CommandDescription("Gives a link to BofSs webpage and the total score of the squadron. (!squadronsum BofSs)")]
        //private async Task HandleSquadronSumCommand(SocketMessage message)
        //{
        //    string content = message.Content.Trim();
        //    string input = content.Substring("!squadronsum ".Length);

        //    if (input == "Cadet" || input == "BofSs" || input == "Academy")
        //    {
        //        message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

        //        Commands scrapeAllAndPopulate = new Commands();
        //        SquadronObj squadronObject = new SquadronObj();

        //        squadronObject = scrapeAllAndPopulate.validateSquadron(input);

        //        squadronObject = await scrapeAllAndPopulate.populateScore(squadronObject).ConfigureAwait(true);

        //        await message.Channel.SendMessageAsync("Squadron  Name: " + squadronObject.SquadronName + ". \rSquadron URL: " + squadronObject.url).ConfigureAwait(true);

        //        var chnl = message.Channel as IMessageChannel;



        //        await chnl.SendMessageAsync("-");

        //        await chnl.SendMessageAsync("Total Score: " + squadronObject.Score.ToString());
        //    }
        //    else
        //    {
        //        await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, or Academy.");
        //    }
        //}

        [CommandDescription("Player count, totals score, and each player's score. (!totals BofSs.")]
        private async Task HandleTotalsCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string input = content.Substring("!totals ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy")
            {
                await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

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

        [CommandDescription("Scrapes a squadron & writes the info to a file.")]
        private async Task HandleWriteSqdCommand(SocketMessage message)
        {
            // Implementation for the !writesqd command
            string input = message.Content.Substring("!writesqd ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy" || input == "BriSs" || input == "BufSs")
            {
                var chnl = message.Channel as IMessageChannel;
                await chnl.SendMessageAsync("Scraping and writing - please hold...");
                Commands command = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = command.validateSquadron(input);
                squadronObject = await command.populateScore(squadronObject).ConfigureAwait(true);
                squadronObject = await command.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


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

                    await chnl.SendMessageAsync("complete!");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, BufSs, BriSs or Academy.");
            }
        }

        private async Task Handle5MinuteWriteTimer(String message)
        {
            // Implementation for the !writesqd command
            //string input = message.Substring("!writesqd ".Length);
            string input = message;

            ITextChannel chnl = _client.GetChannel(EsperBotTestingChannel) as ITextChannel;

            if (input == "Cadet" || input == "BofSs" || input == "Academy" || input == "BriSs")
            {

                //var chnl = message.Channel as IMessageChannel;
                //await chnl.SendMessageAsync("Scraping and writing - please hold...");
                Commands command = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = command.validateSquadron(input);
                squadronObject = await command.populateScore(squadronObject).ConfigureAwait(true);
                squadronObject = await command.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

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
                    //await chnl.SendMessageAsync("completed the write.");
                }
            }
            else
            {
                await chnl.SendMessageAsync("Squadron needs to be Cadet, BofSs, BriSs or Academy.");
            }
        }

        [CommandDescription("Reads the most recently saved squadron file and then prints all players & points. !readsqd BofSs")]
        private async Task HandleReadSqdCommand(SocketMessage message)
        {
            // Implementation for the !readsqd command
            string input = message.Content.Substring("!readsqd ".Length);

            if (input == "Cadet" || input == "BofSs" || input == "Academy" || input == "BufSs" || input == "BriSs")
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
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, BufSs, BriSs, or Academy.");
            }
        }

        [CommandDescription("Lists top 20 players & how many points they have.")]
        private async Task HandleTop20Command(SocketMessage message)
        {
            string content = message.Content.Trim();
            string input = content.Substring("!top20 ".Length);

            if (new[] { "Cadet", "BofSs", "Academy", "BufSs", "Early", "RO6", "AVR", "ILWI", "iNut", "SKAL", "NEURO", "LEDAC", "WeBak", "TFedz", "B0AR", "SOFUA", "AFI", "TEHb", "IRAN","BriSs","EXLY", "ASP1D", "Nrst", "IAVRI", "R6PL", "EPRO", "CLIM", "VaVic" }.Contains(input))
            {
                await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

                Commands Command = new Commands();
                SquadronObj squadronObject = new SquadronObj();

                squadronObject = Command.validateSquadron(input);

                squadronObject = await Command.scrapeAllAndPopulate(squadronObject);

                var chnl = message.Channel as IMessageChannel;

                if (squadronObject.Players.Count == 0)
                {
                    await chnl.SendMessageAsync("No players found for " + input + ".");
                    return;
                }

                // Sort players by score in descending order
                List<Player> top20Players = squadronObject.Players.OrderByDescending(p => p.PersonalClanRating).Take(20).ToList();

                await Command.PrintTop20Players(chnl, squadronObject, top20Players);

            }
            else
            {
                await message.Channel.SendMessageAsync("I didn't recognize your input. This command is case-sensitive, BTW.");
            }
        }

        [CommandDescription("Examines the last 2 written files & lists joiners & leavers. !compare BofSs")]
        private async Task HandleCompareCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string squadronName = content.Substring("!compare ".Length);

            if (squadronName == "Cadet" || squadronName == "BofSs" || squadronName == "Academy" || squadronName == "BriSs" || squadronName == "BufSs")
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
                await message.Channel.SendMessageAsync("Squadron needs to be Cadet, BofSs, BufSs, BriSs or Academy.");
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

        [CommandDescription("Picks a gamemode, type, and BR.  Don't like what it chooses? Spam it until you get one you like.")]
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

        [CommandDescription("!setBR <BR> - Changes divider channels to new BR.")]
        private async Task HandleSetBRCommand(SocketMessage message)
        {
            string content = message.Content;
            string[] parts = content.Split(' ');

            if (parts.Length < 2)
            {
                await message.Channel.SendMessageAsync("Usage: !setbr <value>");
                return;
            }

            string newValue = parts[1];

            // These are your target channel IDs
            ulong[] channelIds = {
        876561486431027250,
        865803859594051634,
        1310053658515345418
    };

            // Get the guild from the channel
            if (message.Channel is SocketGuildChannel guildChannel)
            {
                SocketGuild guild = guildChannel.Guild;

                foreach (ulong id in channelIds)
                {
                    SocketTextChannel channel = guild.GetTextChannel(id);
                    if (channel != null)
                    {
                        await channel.ModifyAsync(props => props.Name = $"[------- SRE {newValue} -------]");
                    }
                }

                await message.Channel.SendMessageAsync($"Channels updated to SRE {newValue}");
            }
            else
            {
                await message.Channel.SendMessageAsync("This command must be used in a server channel.");
            }
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

            if (content.StartsWith("!commands") || content.StartsWith("!help"))
            {

                MethodInfo[] methods = typeof(Program).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
    .Where(method => method.Name.StartsWith("Handle") && method.Name.EndsWith("Command") && method.GetParameters().Length == 1 &&
                     (method.GetParameters()[0].ParameterType == typeof(SocketMessage) || method.GetParameters()[0].ParameterType == typeof(IMessageChannel)))
    .ToArray();


                List<string> commandList = new List<string>();

                foreach (MethodInfo method in methods)
                {
                    string methodName = method.Name;
                    string command = "!" + methodName.Substring("Handle".Length, methodName.Length - "HandleCommand".Length);

                    var descriptionAttribute = method.GetCustomAttribute<CommandDescriptionAttribute>();
                    if (descriptionAttribute != null)
                    {
                        string description = descriptionAttribute.Description;
                        string paddedCommand = command.PadRight(20); // Adjust the width as needed
                        commandList.Add($"{paddedCommand} - {description}");
                    }
                    else
                    {
                        commandList.Add(command);
                    }
                }


                string commandsText = string.Join("\n", commandList);

                await message.Channel.SendMessageAsync("Available commands:\n```" + commandsText + "```");
            }
        }

        [CommandDescription("Who is online & how many points do they have? If player not found, give player name & Discord ID to Esper.")]
        //        private async Task HandleQpointsCommand(SocketMessage message)
        //        {
        //            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

        //            Commands commands = new Commands();

        //            SquadronObj squadronObject = new SquadronObj();
        //            squadronObject = commands.validateSquadron("BofSs");


        //            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

        //            ulong channelId = (ulong)700529928948678777;
        //            IVoiceChannel voiceChannel = _client.GetChannel(channelId) as IVoiceChannel;

        //            // Create a list to store member usernames
        //            List<string> playerList = new List<string>();
        //            var playerInfoList = new List<(string PlayerName, string Points)>();

        //            await Task.Delay(1000);
        //            playerList = await commands.GeneratePlayerList(_client, voiceChannel.Id, playerList);

        //            string[] itemsToJoin = playerList.Take(playerList.Count - 1).ToArray();
        //            string playerListString = string.Join("", itemsToJoin).ToString();

        //            await Task.Delay(1000);
        //            await commands.UpdatePlayerIDs(squadronObject);
        //            await Task.Delay(1000);

        //            StringBuilder responseBuilder = new StringBuilder();

        //            foreach (var playerName in playerList)
        //            {
        //                if (ulong.TryParse(playerName.Split(' ')[0], out ulong discordId))
        //                {
        //                    Player player = squadronObject.Players.FirstOrDefault(p => p.DiscordID == discordId);

        //                    if (player != null)
        //                    {
        //                        playerInfoList.Add((player.PlayerName, player.PersonalClanRating.ToString()));
        //                    }
        //                    else
        //                    {
        //                        ulong userId = discordId;
        //                        var user = _client.GetUser(userId);

        //                        if (user != null)
        //                        {
        //                            playerInfoList.Add((discordId.ToString(), $"Player not found - {user.Username}"));
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    playerInfoList.Add((playerName, "Invalid format"));
        //                }
        //            }

        //;
        //            playerInfoList = playerInfoList.OrderByDescending(p =>
        //            {
        //                if (int.TryParse(p.Points, out int points))
        //                {
        //                    return points;
        //                }
        //                return 0; // Set a default value for invalid points
        //            }).ToList();



        //            // Iterate through the sorted list and build your response
        //            foreach (var playerInfo in playerInfoList)
        //            {
        //                responseBuilder.AppendLine($"{playerInfo.PlayerName,-20}: {playerInfo.Points,-6} points");
        //            }


        //            await commands.SendLongContentAsEmbedAsync(message.Channel, responseBuilder.ToString()); //Player Names and Points
        //            await commands.SendLongContentAsEmbedAsync(message.Channel, playerListString); //IDs and Discord Names

        //            //await message.Channel.SendMessageAsync(response);
        //        }
        //private async Task Handle2QpointsCommand(SocketMessage message)
        //{
        //    await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

        //    Commands commands = new Commands();

        //    // Validate and scrape for BofSs
        //    SquadronObj squadronObject1 = commands.validateSquadron("BofSs");
        //    if (squadronObject1 == null)
        //    {
        //        await message.Channel.SendMessageAsync("Squadron 'BofSs' not found.");
        //        return;
        //    }
        //    squadronObject1 = await commands.scrapeAllAndPopulate(squadronObject1).ConfigureAwait(true);

        //    // Validate and scrape for BufSs
        //    SquadronObj squadronObject2 = commands.validateSquadron("BufSs");
        //    if (squadronObject2 == null)
        //    {
        //        await message.Channel.SendMessageAsync("Squadron 'BufSs' not found.");
        //        return;
        //    }
        //    squadronObject2 = await commands.scrapeAllAndPopulate(squadronObject2).ConfigureAwait(true);

        //    // Validate and scrape for BriSs
        //    SquadronObj squadronObject3 = commands.validateSquadron("BriSs");
        //    if (squadronObject3 == null)
        //    {
        //        await message.Channel.SendMessageAsync("Squadron 'BriSs' not found.");
        //        return;
        //    }
        //    squadronObject3 = await commands.scrapeAllAndPopulate(squadronObject3).ConfigureAwait(true);

        //    ulong channelId = (ulong)700529928948678777;
        //    IVoiceChannel voiceChannel = _client.GetChannel(channelId) as IVoiceChannel;

        //    List<string> playerList = new List<string>();
        //    var playerInfoList = new List<(string PlayerName, string Points)>();

        //    await Task.Delay(1000);
        //    playerList = await commands.GeneratePlayerList(_client, voiceChannel.Id, playerList);

        //    string[] itemsToJoin = playerList.Take(playerList.Count - 1).ToArray();
        //    string playerListString = string.Join("", itemsToJoin).ToString();

        //    await Task.Delay(1000);
        //    await commands.UpdatePlayerIDs(squadronObject1);
        //    await commands.UpdatePlayerIDs(squadronObject2);
        //    await commands.UpdatePlayerIDs(squadronObject3);
        //    await Task.Delay(1000);

        //    StringBuilder responseBuilder = new StringBuilder();

        //    foreach (var playerName in playerList)
        //    {
        //        if (ulong.TryParse(playerName.Split(' ')[0], out ulong discordId))
        //        {
        //            Player player1 = squadronObject1.Players.FirstOrDefault(p => p.DiscordID == discordId);
        //            Player player2 = squadronObject2.Players.FirstOrDefault(p => p.DiscordID == discordId);
        //            Player player3 = squadronObject3.Players.FirstOrDefault(p => p.DiscordID == discordId);

        //            if (player1 != null)
        //            {
        //                playerInfoList.Add((player1.PlayerName, player1.PersonalClanRating.ToString()));
        //            }
        //            else if (player2 != null)
        //            {
        //                playerInfoList.Add((player2.PlayerName, player2.PersonalClanRating.ToString()));
        //            }
        //            else if (player3 != null)
        //            {
        //                playerInfoList.Add((player3.PlayerName, player3.PersonalClanRating.ToString()));
        //            }
        //            else
        //            {
        //                ulong userId = discordId;
        //                var user = _client.GetUser(userId);

        //                if (user != null)
        //                {
        //                    playerInfoList.Add((discordId.ToString(), $"Player not found - {user.Username}"));
        //                }
        //            }
        //        }
        //        else
        //        {
        //            playerInfoList.Add((playerName, "Invalid format"));
        //        }
        //    }

        //    playerInfoList = playerInfoList.OrderByDescending(p =>
        //    {
        //        if (int.TryParse(p.Points, out int points))
        //        {
        //            return points;
        //        }
        //        return 0;
        //    }).ToList();

        //    foreach (var playerInfo in playerInfoList)
        //    {
        //        responseBuilder.AppendLine($"{playerInfo.PlayerName,-20}: {playerInfo.Points,-6} points");
        //    }

        //    await commands.SendLongContentAsEmbedAsync(message.Channel, responseBuilder.ToString());
        //    await commands.SendLongContentAsEmbedAsync(message.Channel, playerListString);
        //}




//private async Task Handle3QpointsCommand(SocketMessage message)
//    {
//        await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

//        Commands commands = new Commands();

//        // Validate and scrape for squadrons
//        SquadronObj squadronObject1 = commands.validateSquadron("BofSs");
//        SquadronObj squadronObject2 = commands.validateSquadron("BufSs");
//        SquadronObj squadronObject3 = commands.validateSquadron("BriSs");

//        if (squadronObject1 == null || squadronObject2 == null || squadronObject3 == null)
//        {
//            await message.Channel.SendMessageAsync("One or more squadrons not found.");
//            return;
//        }

//        squadronObject1 = await commands.scrapeAllAndPopulate(squadronObject1).ConfigureAwait(true);
//        squadronObject2 = await commands.scrapeAllAndPopulate(squadronObject2).ConfigureAwait(true);
//        squadronObject3 = await commands.scrapeAllAndPopulate(squadronObject3).ConfigureAwait(true);

//        if (message.Channel is SocketGuildChannel guildChannel)
//        {
//            SocketGuild guild = guildChannel.Guild;

//            HashSet<ulong> uniqueUserIds = new HashSet<ulong>();
//            List<IGuildUser> allUsersInVoice = new List<IGuildUser>();

//            foreach (SocketVoiceChannel voiceChannel in guild.VoiceChannels)
//            {
//                foreach (IGuildUser user in voiceChannel.Users)
//                {
//                    if (user is SocketGuildUser socketUser && socketUser.VoiceState != null && uniqueUserIds.Add(user.Id))
//                    {
//                        allUsersInVoice.Add(user);
//                    }
//                }
//            }

//            StringBuilder responseBuilder = new StringBuilder();
//            var playerInfoList = new List<(string PlayerName, string Points)>();

//            await commands.UpdatePlayerIDs(squadronObject1);
//            await commands.UpdatePlayerIDs(squadronObject2);
//            await commands.UpdatePlayerIDs(squadronObject3);

//            foreach (IGuildUser user in allUsersInVoice)
//            {
//                if (user.IsBot) { continue; }
//                string discordName = RemoveEmojis(user.DisplayName);
//                string cleanDiscordName = discordName.Trim();

//                Player player1 = squadronObject1.Players.FirstOrDefault(p => CleanPlayerName(p.PlayerName).Equals(cleanDiscordName, StringComparison.OrdinalIgnoreCase));
//                Player player2 = squadronObject2.Players.FirstOrDefault(p => CleanPlayerName(p.PlayerName).Equals(cleanDiscordName, StringComparison.OrdinalIgnoreCase));
//                Player player3 = squadronObject3.Players.FirstOrDefault(p => CleanPlayerName(p.PlayerName).Equals(cleanDiscordName, StringComparison.OrdinalIgnoreCase));

//                if (player1 != null)
//                {
//                    playerInfoList.Add((user.DisplayName, player1.PersonalClanRating.ToString()));
//                }
//                else if (player2 != null)
//                {
//                    playerInfoList.Add((user.DisplayName, player2.PersonalClanRating.ToString()));
//                }
//                else if (player3 != null)
//                {
//                    playerInfoList.Add((user.DisplayName, player3.PersonalClanRating.ToString()));
//                }
//                else
//                {
//                    playerInfoList.Add((user.DisplayName, "Player not found"));
//                }
//            }

//            playerInfoList = playerInfoList.OrderByDescending(p =>
//            {
//                if (int.TryParse(p.Points, out int points))
//                {
//                    return points;
//                }
//                return 0;
//            }).ToList();

//            foreach (var playerInfo in playerInfoList)
//            {
//                responseBuilder.AppendLine($"{playerInfo.PlayerName,-20}: {playerInfo.Points,-6} points");
//            }

//            await commands.SendLongContentAsEmbedAsync(message.Channel, responseBuilder.ToString());
//        }
//        else
//        {
//            await message.Channel.SendMessageAsync("This command can only be used in a server channel.");
//            return;
//        }
//    }

    private string RemoveEmojis(string text)
    {

            string emojiRegex = @"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])";
            return Regex.Replace(text, emojiRegex, "");

        }

        private string CleanPlayerName(string playerName)
    {
        return playerName.Trim();
    }









    private async Task HandleTop20NoArgCommand(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("I need a squadron, too.  You can enter \"Cadet\", \"BofSs\", \"Academy\", \"Early\", \"RO6\", \"AVR\", \"ILWI\", \"iNut\", \"SKAL\", \"NEURO\", \"LEDAC\", \"B0AR\", \"SOFUA\", \"TFedz\",\"AFI\",\"TEHb\",\"IRAN\",\"BriSs\",\"EXLY\",\"ASP1D\",\"Nrst\",\"IAVRI\",\"R6PL\",\"EPRO\",\"CLIM\" - This is case-sensitive");
        }

        //[CommandDescription("turns on & off login/logoff/move notifs.")]
        private async Task HandleTrackVoiceUpdatesCommand(SocketMessage message)
        {
            if (message.Content == "!trackvoiceupdates on")
            {
                trackVoiceUpdates = true;
                await message.Channel.SendMessageAsync("OK, turning on voice channel update tracking.");
            }

            else if (message.Content == "!trackvoiceupdates off")
            {
                trackVoiceUpdates = false;
                await message.Channel.SendMessageAsync("OK, turning off voice channel update tracking.");
            }
            else
            {
                await message.Channel.SendMessageAsync("Sorry, the only options are \"on\" and \"off\".  \nThe current status of tracking Voice Channel User Updates is: " + trackVoiceUpdates.ToString());

            }

        }

        private async Task HandleresetsessionwlvariablesCom(SocketMessage message)
            
        {
            if (message.Content == "!resetsessionwlvariables")
            {
                ResetWLSessionVariables();
                await message.Channel.SendMessageAsync("reset");   
            }

        }

        [CommandDescription("These are commands that you shouldn't use.")]
        private async Task HandleDirectBotCommandsCommand(SocketMessage message)
        {
           
                //await message.Channel.SendMessageAsync("Win/Loss count for this session is: " + winCounter + "-" + lossCounter + ".");
                

                await message.Channel.SendMessageAsync($"The following commands are available for directly changing bot variables.  Please do NOT use the commands unless you know what you're doing.\n!resetsessionwlvariables\r\n!SetWinLoss\r\n!2SetWinLoss\n!SetStartingScore\r\n!2SetStartingScore\r\n!SquadronTotalScore\r\n!2SquadronTotalScore");
            
        }
        [CommandDescription("Displays the win/loss counts for this SRE session.")]
        private async Task HandleRecordCommand(SocketMessage message)
        {
            if (message.Content == "!record")
            {
                //await message.Channel.SendMessageAsync("Win/Loss count for this session is: " + winCounter + "-" + lossCounter + ".");
                SquadronObj squadron5m = await Webscraper.ScrapeCheck($"!check BofSs");

                await message.Channel.SendMessageAsync($"Win/Loss count for this session is: {midSessionWinsCounter}-{midSessionLossesCounter} ({squadron5m.Score - startOfSessionPoints}).");
             }
        }
        [CommandDescription("2")]
        private async Task Handle2RecordCommand(SocketMessage message)
        {
            if (message.Content == "!2record")
            {
                //await message.Channel.SendMessageAsync("Win/Loss count for this session is: " + winCounter + "-" + lossCounter + ".");
                SquadronObj squadron5m = await Webscraper.ScrapeCheck($"!check BufSs");

                await message.Channel.SendMessageAsync($"Win/Loss count for this session is: {midSessionWinsCounterBufSs}-{midSessionLossesCounterBufSs} ({squadron5m.Score - startOfSessionPointsBufSs}).");
            }
        }
        
        //[CommandDescription("Listplayers <over> / <under> <points> - example: \"!listplayers under 1500\"")]
        //private async Task HandleListplayersCommand(SocketMessage message)
        //{
        //    string content = message.Content.Trim();

        //    // Split the input string into words
        //    string[] words = content.Split(' ');

        //    // Check that the first word is "!listplayers"
        //    if (words[0] != "!listplayers")
        //    {
        //        // If the first word is not "!listplayers", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the second word is either "over" or "under"
        //    if (words[1] != "over" && words[1] != "under")
        //    {
        //        // If the second word is not "over" or "under", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the third word is a valid number
        //    int points;
        //    if (!int.TryParse(words[2], out points))
        //    {
        //        // If the third word is not a valid number, then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    string overUnder = words[1];

        //    await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

        //    Commands commands = new Commands();
        //    SquadronObj squadronObject = new SquadronObj();

        //    squadronObject = commands.validateSquadron("BofSs");

        //    var chnl = message.Channel as IMessageChannel;

        //    chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

        //    squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
        //    squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


        //    //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
        //    //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
        //    //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

        //    commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        //}

        //[CommandDescription("2")]
        //private async Task Handle2ListplayersCommand(SocketMessage message)
        //{
        //    string content = message.Content.Trim();

        //    // Split the input string into words
        //    string[] words = content.Split(' ');

        //    // Check that the first word is "!listplayers"
        //    if (words[0] != "!2listplayers")
        //    {
        //        // If the first word is not "!listplayers", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the second word is either "over" or "under"
        //    if (words[1] != "over" && words[1] != "under")
        //    {
        //        // If the second word is not "over" or "under", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the third word is a valid number
        //    int points;
        //    if (!int.TryParse(words[2], out points))
        //    {
        //        // If the third word is not a valid number, then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    string overUnder = words[1];

        //    await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

        //    Commands commands = new Commands();
        //    SquadronObj squadronObject = new SquadronObj();

        //    squadronObject = commands.validateSquadron("BufSs");



        //    var chnl = message.Channel as IMessageChannel;

        //    chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

        //    squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
        //    squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


        //    //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
        //    //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
        //    //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

        //    commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        //}

        //[CommandDescription("3")]
        //private async Task Handle3ListplayersCommand(SocketMessage message)
        //{
        //    string content = message.Content.Trim();

        //    // Split the input string into words
        //    string[] words = content.Split(' ');

        //    // Check that the first word is "!listplayers"
        //    if (words[0] != "!3listplayers")
        //    {
        //        // If the first word is not "!listplayers", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the second word is either "over" or "under"
        //    if (words[1] != "over" && words[1] != "under")
        //    {
        //        // If the second word is not "over" or "under", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the third word is a valid number
        //    int points;
        //    if (!int.TryParse(words[2], out points))
        //    {
        //        // If the third word is not a valid number, then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    string overUnder = words[1];

        //    await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

        //    Commands commands = new Commands();
        //    SquadronObj squadronObject = new SquadronObj();

        //    squadronObject = commands.validateSquadron("BriSs");



        //    var chnl = message.Channel as IMessageChannel;

        //    chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

        //    squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
        //    squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


        //    //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
        //    //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
        //    //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

        //    commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        //}
        //[CommandDescription("4")]
        //private async Task Handle4ListplayersCommand(SocketMessage message)
        //{
        //    string content = message.Content.Trim();

        //    // Split the input string into words
        //    string[] words = content.Split(' ');

        //    // Check that the first word is "!listplayers"
        //    if (words[0] != "!4listplayers")
        //    {
        //        // If the first word is not "!listplayers", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the second word is either "over" or "under"
        //    if (words[1] != "over" && words[1] != "under")
        //    {
        //        // If the second word is not "over" or "under", then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    // Check that the third word is a valid number
        //    int points;
        //    if (!int.TryParse(words[2], out points))
        //    {
        //        // If the third word is not a valid number, then the input is invalid
        //        await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
        //        return;
        //    }

        //    string overUnder = words[1];

        //    await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

        //    Commands commands = new Commands();
        //    SquadronObj squadronObject = new SquadronObj();

        //    squadronObject = commands.validateSquadron("EXLY");



        //    var chnl = message.Channel as IMessageChannel;

        //    chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

        //    squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
        //    squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


        //    //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
        //    //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
        //    //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

        //    commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        //}

        [CommandDescription("Listplayers [<over> | <under>] <squadron> <points> - example: \"!listplayers BofSs under 1500\"")]
        private async Task HandleListplayersCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string[] words = content.Split(' ');

            if (words.Length < 4) // Require at least 4 words: !listplayers <squadron> <over/under> <points>
            {
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <squadron> [<over>|<under>] <points> - I didn't get 4 words.");
                return;
            }

            string squadronName = words[1]; // Get the squadron name from the second word
            string overUnder = words[2];
            int points;

            if (!int.TryParse(words[3], out points)) // Get points from the fourth word
            {
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <squadron> <over> / <under> <points> - I didn't get points from the 4th word.");
                return;
            }

            if (overUnder != "over" && overUnder != "under")
            {
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <squadron> <over> / <under> <points> - I didn't see 'over' or 'under' in the right spot.");
                return;
            }

            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

            Commands commands = new Commands();
            SquadronObj squadronObject = new SquadronObj();

            squadronObject = commands.validateSquadron(squadronName); // Use the provided squadron name

            if (squadronObject == null) // Check if squadron is valid
            {
                await message.Channel.SendMessageAsync($"Squadron '{squadronName}' not found.");
                return;
            }

            var chnl = message.Channel as IMessageChannel;

            await chnl.SendMessageAsync("Players in " + squadronName + " with score " + overUnder + " " + points + ":");

            squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

            commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);
        }


        [CommandDescription("EOS <squadron> [<over> | <under>] <points> - example: \"!listplayers BofSs under 1500\"")]
        private async Task HandleEOSCommand(SocketMessage message)
        {
            string content = message.Content.Trim();
            string[] words = content.Split(' ');

            if (words.Length < 4) // Require at least 4 words: !listplayers <squadron> <over/under> <points>
            {
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <squadron> [<over>|<under>] <points> - I didn't get 4 words.");
                return;
            }

            string squadronName = words[1]; // Get the squadron name from the second word
            string overUnder = words[2];
            int points;

            if (!int.TryParse(words[3], out points)) // Get points from the fourth word
            {
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <squadron> <over> / <under> <points> - I didn't get points from the 4th word.");
                return;
            }

            if (overUnder != "over" && overUnder != "under")
            {
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <squadron> <over> / <under> <points> - I didn't see 'over' or 'under' in the right spot.");
                return;
            }

            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

            Commands commands = new Commands();
            SquadronObj squadronObject = new SquadronObj();

            squadronObject = commands.validateSquadron(squadronName); // Use the provided squadron name

            if (squadronObject == null) // Check if squadron is valid
            {
                await message.Channel.SendMessageAsync($"Squadron '{squadronName}' not found.");
                return;
            }

            var chnl = message.Channel as IMessageChannel;

            await chnl.SendMessageAsync("Players in " + squadronName + " with score " + overUnder + " " + points + " and their join date:");

            squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

            commands.printPlayersOverUnder2(chnl, squadronObject, overUnder, points);
        }


        //[CommandDescription("Displays the last ten SRE session counts.")]
        private async Task HandleLastTenCommand(SocketMessage message)
        {
            string content = message.ToString().ToLower();
            if (content == "!lastten")
            {

                string fileName = "SREWinLossRecords.txt";

                // Check if the file exists
                if (File.Exists(fileName))
                {
                    // Read all lines from the file
                    string[] lines = File.ReadAllLines(fileName);

                    // Calculate how many lines you want to retrieve (last ten or all if less than ten)
                    int numberOfLinesToRetrieve = Math.Min(10, lines.Length);

                    // Get the last ten (or fewer) lines
                    //string[] lastEntries = lines.TakeLast(numberOfLinesToRetrieve).ToArray();
                    string[] lastEntries = lines.Skip(Math.Max(0, lines.Length - numberOfLinesToRetrieve)).Take(numberOfLinesToRetrieve).ToArray();

                    string combinedEntries = string.Join("\n", lastEntries);

                    await message.Channel.SendMessageAsync(("..." + combinedEntries));
                }
                else
                {
                    await message.Channel.SendMessageAsync("The file 'SREWinLossRecords.txt' does not exist.");
                }


            }


        }

        [CommandDescription("!SetWinLoss <Num/Num> of current session.")]
        private async Task HandleSetWinLossCom(SocketMessage message)
        {
            string content = message.ToString().ToLower();
            if (message.Author.Id == 308128406699245568 || ((SocketGuildUser)message.Author).Roles.Any(r => r.Id == officerRoleId))
            {

                content = message.Content.Trim();
                string[] parts = content.Split(' ');

                if (parts.Length == 2 && parts[0].Equals("!setwinloss", StringComparison.OrdinalIgnoreCase))
                {
                    
                        string[] counters = parts[1].Split('/');

                        if (counters.Length == 2)
                        {
                            if (int.TryParse(counters[0], out int newWins) && int.TryParse(counters[1], out int newLosses))
                            {
                                midSessionWinsCounter = newWins;
                                midSessionLossesCounter = newLosses;

                                await message.Channel.SendMessageAsync($"Win and Loss counters updated. Wins: {midSessionWinsCounter}, Losses: {midSessionLossesCounter}");
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("Invalid counter values. Please use the format '!setwinloss wins/losses'.");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("Invalid format. Please use the format '!setwinloss wins/losses'.");
                        }
                    
                }
                else
                {
                    await message.Channel.SendMessageAsync("Invalid command format. Use '!setwinloss wins/losses'.");
                }
            }
            else
            {
               await message.Channel.SendMessageAsync("C'mon, now, only Esper or an officer has that power.");
            }
        }

        private async Task Handle2SetWinLossCom(SocketMessage message)
        {
            string content = message.ToString().ToLower();
            if (message.Author.Id == 308128406699245568 || ((SocketGuildUser)message.Author).Roles.Any(r => r.Id == officerRoleId))
            {

                content = message.Content.Trim();
                string[] parts = content.Split(' ');

                if (parts.Length == 2 && parts[0].Equals("!2setwinloss", StringComparison.OrdinalIgnoreCase))
                {
                    
                        string[] counters = parts[1].Split('/');

                        if (counters.Length == 2)
                        {
                            if (int.TryParse(counters[0], out int newWins) && int.TryParse(counters[1], out int newLosses))
                            {
                                midSessionWinsCounterBufSs = newWins;
                                midSessionLossesCounterBufSs = newLosses;

                                await message.Channel.SendMessageAsync($"BufSs Win and Loss counters updated. Wins: {midSessionWinsCounterBufSs}, Losses: {midSessionLossesCounterBufSs}");
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("Invalid counter values. Please use the format '!setwinloss wins/losses'.");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("Invalid format. Please use the format '!2setwinloss wins/losses'.");
                        }
                    
                }
                else
                {
                    await message.Channel.SendMessageAsync("Invalid command format. Use '!setwinloss wins/losses'.");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("C'mon, now, only Esper or an officer has that power.");
            }
        }

        private async Task HandleSetStartingScoreCom(SocketMessage message)
        {
            string content = message.ToString().ToLower();
            if (message.Author.Id == 308128406699245568 || ((SocketGuildUser)message.Author).Roles.Any(r => r.Id == officerRoleId))
            {
                content = message.Content.Trim();
                string[] parts = content.Split(' ');

                if (parts.Length == 2 && parts[0].Equals("!setstartingscore", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(parts[1], out int score))
                    {
                        startOfSessionPoints = score;
                        await message.Channel.SendMessageAsync($"Start of session points set to: {startOfSessionPoints}.");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Invalid value. Please use the format '!setstartingscore <number>' with a valid integer.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("Invalid format. Please use the format '!setstartingscore <number>'.");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("C'mon, now, only Esper or an officer has that power.");
            }
        }

        private async Task Handle2SetStartingScoreCom(SocketMessage message)
        {
            string content = message.ToString().ToLower();
            if (message.Author.Id == 308128406699245568 || ((SocketGuildUser)message.Author).Roles.Any(r => r.Id == officerRoleId))
            {
                content = message.Content.Trim();
                string[] parts = content.Split(' ');

                if (parts.Length == 2 && parts[0].Equals("!2setstartingscore", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(parts[1], out int score))
                    {
                        startOfSessionPointsBufSs = score;
                        await message.Channel.SendMessageAsync($"Start of session points for BufSs set to: {startOfSessionPointsBufSs}.");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Invalid value. Please use the format '!setstartingscore <number>' with a valid integer.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("Invalid format. Please use the format '!setstartingscore <number>'.");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("C'mon, now, only Esper or an officer has that power.");
            }
        }

        [CommandDescription("listalts - Shows alts and points on each.")]
        private async Task HandleListAltsCommand(SocketMessage message)
        {
            string content = message.Content.Trim();

            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments. Results will be sent to the <#1165452109513244673> channel");

            Commands commands = new Commands();
            SquadronObj squadronObject = new SquadronObj();

            squadronObject = commands.validateSquadron("BofSs");

            var chnl = _client.GetChannel(esperbotchannel) as IMessageChannel;

            chnl.SendMessageAsync("Alts:");

            squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

            string altListFilePath = "AltList.txt"; // Replace with the correct file path

            SquadronObj squadronObjectOfAlts = new SquadronObj();
            squadronObjectOfAlts.Players = new List<Player>();

            // Read the lines from the AltList.txt file
            string[] altNames = File.ReadAllLines(altListFilePath);

            // Replace 'squadronObject' with your SquadronObj instance
            foreach (var altName in altNames)
            {
                // Find the player in squadronObject by their name
                Player player = squadronObject.Players.FirstOrDefault(p => p.PlayerName.Equals(altName, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                {
                    // Append the alt name and points to the response
                    //string response = $"{altName}: {player.PersonalClanRating} points";
                    //await ReplyAsync(response);
                    //player temp = new SquadronObject.Player();
                    //squadronObjectOfAlts.Players.Add(player);

                    Player copiedPlayer = new Player
                    {
                        PlayerName = player.PlayerName,
                        PersonalClanRating = player.PersonalClanRating,
                        // Copy other properties you need
                    };

                    // Add the copied player to the target squadron
                    squadronObjectOfAlts.Players.Add(copiedPlayer);


                }
                else
                {
                    // Handle the case where the player is not found
                    //await ReplyAsync($"{altName}: Player not found");
                }
            }

            commands.printPlayersOverUnder(chnl, squadronObjectOfAlts, "under", 2100);

        }

       // [CommandDescription("Updates the daily total to the current total.")]
        private async Task HandleSquadronTotalScoreCom(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("OK, " + squadronTotalScore + " is the current value of SquadronTotalScore.  Also, I am executing the populate now.");

            Commands commands = new Commands();
            SquadronObj sqdObj = new SquadronObj();
            sqdObj.url = "https://warthunder.com/en/community/claninfo/Band%20Of%20Scrubs";
            await commands.populateScore(sqdObj);
            squadronTotalScore = sqdObj.Score;

            await message.Channel.SendMessageAsync("" + squadronTotalScore);

        }

        [CommandDescription("2")]
        private async Task Handle2SquadronTotalScoreCom(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("OK, " + squadronTotalScoreBufSs + " is the current value of SquadronTotalScoreBufSs.  Also, I am executing the populate now.");

            Commands commands = new Commands();
            SquadronObj sqdObj2 = new SquadronObj();
            sqdObj2.url = "https://warthunder.com/en/community/claninfo/Bunch%20of%20Scrubs?69";
            await commands.populateScore(sqdObj2);
            squadronTotalScoreBufSs = sqdObj2.Score;

            await message.Channel.SendMessageAsync("" + squadronTotalScoreBufSs);

        }


        [CommandDescription("Dislpays live TopSquadrons stats. Useful for manual, real-time tracking.")]
        private async Task HandleScrapeCommand(SocketMessage message)
        {

            //SquadronObj[] sqbObjList = new SquadronObj();
            SquadronObj[] content = await Webscraper.TestScrape2(); // Call the TestScrape method

            const int maxEmbedLength = 4096;
            const int maxChunkLength = 2000;

            //if (content.Length <= maxEmbedLength)
            //{
            //    // If the content fits within the limit, send it as a single embedded message
            //    await message.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription($"```{content}```").Build());
            //}
            //else
            //{
            //    Console.WriteLine("content.length is greater than or equal to maxEmbedLength.");
            //    await message.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription($"content.length is greater than or equal to maxEmbedLength.").Build());
            //}

            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.AppendLine("   Name Wins Losses Total  Pts");

            foreach (var squadronObj in content)
            {
                string paddedPos = squadronObj.Pos.ToString().PadRight(2, ' ');
                string paddedName;
                string paddedWins = squadronObj.Wins.ToString().PadLeft(3, ' ');
                string paddedLosses = squadronObj.Losses.ToString().PadLeft(3, ' '); ;

                if (squadronObj.Pos < 10)
                {

                    paddedName = squadronObj.SquadronName.PadRight(5, ' ');
                }
                else
                {
                    paddedName = squadronObj.SquadronName.PadRight(5, ' ');
                }

                messageBuilder.AppendLine($"{paddedPos} {paddedName} {paddedWins} & {paddedLosses}. ({squadronObj.BattlesPlayed}). {squadronObj.Score} ");
            }

            //await message.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription(messageBuilder.ToString()).Build());

            await message.Channel.SendMessageAsync($"```{messageBuilder.ToString()}```");

        }

        [CommandDescription("Compares the end-of-session totals to what's live right now. Shows the changes since.")]
        private async Task HandleCompareScrapeCommand(IMessageChannel message)
        {
            Commands commands = new Commands();
            string currentContent = null;

            //loads the most recent TopSquads file into currentContent.
            currentContent = await commands.LoadStringWithMostRecentTopSquad(message);

            // Perform a new scrape.  Returns a list of Squadron Objects as an Array.
            SquadronObj[] newContent = await Webscraper.TestScrape2();

            //returns newcontent, which is an array of SquadronObj's.  But the SquadronObj's in it, after being returned, have populated values for things like WinsChange, ScoreChange, etc.
            SquadronObj[] comparisonResult = commands.CompareContents2(currentContent, newContent);

            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder = await commands.FormatAndSendComparisonResults(newContent);

            await message.SendMessageAsync($"```{messageBuilder.ToString()}```");
        }

        [CommandDescription("Prints current stats.  !check <bufss> or !check <bofss>")]
        private async Task HandleCheckCommand(string command, IMessageChannel channel)
        {
            if (command.ToLower() == "!check bofss" || command.ToLower() == "!check bufss" || command.ToLower() == "!check briss")
            {
                const int maxEmbedLength = 4096;
                const int maxChunkLength = 2000;

                // Activate check process and get the message
                StringBuilder messageBuilder = await ActivateCheckLoadProcess(command);

                // Send the response to the specified channel
                await channel.SendMessageAsync($"```{messageBuilder.ToString()}```");
            }
            else
            {
                await channel.SendMessageAsync("Sorry, I only accept bofss and bufss and briss at this time.");
            }
        }

        // Wrapper method for actual `SocketMessage` handling
        [CommandDescription("Prints current stats.  !check <bufss> or !check <bofss>")]
        private async Task HandleCheckCommand(SocketMessage message)
        {
            await HandleCheckCommand(message.Content, message.Channel);
        }

        private async Task<StringBuilder> ActivateCheckLoadProcess(string content)
        {
            SquadronObj content2 = await Webscraper.ScrapeCheck(content); // Call the TestScrape method. Returns a SquadronObj with populated values for wins/losses/battlesplayed, etc.

            SquadronObj[] squadronArray = new SquadronObj[1];
            squadronArray[0] = content2;

            Commands commands = new Commands();
            StringBuilder messageBuilder = new StringBuilder();
            if (squadronArray[0] != null)
            {

                messageBuilder = await commands.FormatAndSendComparisonResults(squadronArray);

                return messageBuilder;
            }
            else
            {
                return messageBuilder;
            }

        }

        private async Task WriteCheck(String content)
        {
            if (content.ToLower() == "!check bofss" || content.ToLower() == "!check bufss" || content.ToLower() == "!check briss")
            {

                StringBuilder messageBuilder = await ActivateCheckLoadProcess(content);

                //messageBuilder.AppendLine("   Name Wins Losses Total  Pts");
                //await message.Channel.SendMessageAsync($"```{messageBuilder.ToString()}```");
                //write the checkbufss file.

                // Create a file name with the date and time prefix


                string fileName = "CheckFunctionErrorFile.txt";

                if (content.ToLower() == "!check bufss")
                {
                    fileName = "CheckBufSs.txt";
                }
                if (content.ToLower() == "!check bofss")
                {
                    fileName = "CheckBofSs.txt";
                }
                if (content.ToLower() == "!check briss")
                {
                    fileName = "CheckBriSs.txt";
                }


                // Check if the file exists
                if (!File.Exists(fileName))
                {
                    // If the file does not exist, create it
                    using (File.Create(fileName)) { };
                }

                // Open the file for writing
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // Write the win and loss counters to the file


                    //string[] lines = messageBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    //// Skip the first line and join the remaining lines
                    //string contentToWrite = string.Join(Environment.NewLine, lines.Skip(1));
                    //// Write the content to the file
                    ////await File.WriteAllTextAsync(filePath, contentToWrite);
                    //writer.Write(contentToWrite);




                    SquadronObj squadron = await Webscraper.ScrapeCheck(content);

                    // Format to match previous entries exactly (WITH ampersand):
                    string formattedLine = $"{squadron.Pos} {squadron.SquadronName} {squadron.Wins} & {squadron.Losses}. {squadron.BattlesPlayed}. {squadron.Score}";

                    writer.WriteLine(formattedLine);





                }

            }
            else
            {
                Console.WriteLine("error in the else of writecheck.");
            }
        }


        public static (int[], int[]) ReadCheck(string squadronName)
        {
            string fileName = GetCheckFileName(squadronName); // Helper function to get the filename

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"The file '{fileName}' does not exist.");
            }

            string[] lines = File.ReadAllLines(fileName);
            if (lines.Length < 2) // Need at least two lines to compare
            {
                throw new InvalidOperationException("The file doesn't contain enough lines.");
            }

            // Parse the last line
            string lastLine = lines.Last();
            string[] lastParts = lastLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (lastParts.Length < 7) // Now expecting 7 parts
            {
                throw new InvalidOperationException($"Last line '{lastLine}' does not contain enough parts.");
            }
            int lastPos = int.Parse(lastParts[0]);
            string lastName = lastParts[1];
            int lastWins = int.Parse(lastParts[2]);
            int lastLosses = int.Parse(lastParts[4].Trim('.')); // Losses is at index 4
            int lastTotalPlayed = int.Parse(lastParts[5].Trim('.'));
            int lastScore = int.Parse(lastParts[6]);
            int[] lastLineData = new int[] { lastPos, lastWins, lastLosses, lastTotalPlayed, lastScore };

            // Parse the second to last line
            string secondToLastLine = lines[lines.Length - 2];
            string[] secondToLastParts = secondToLastLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (secondToLastParts.Length < 7) // Now expecting 7 parts
            {
                throw new InvalidOperationException($"Second to last line '{secondToLastLine}' does not contain enough parts.");
            }
            int secondToLastPos = int.Parse(secondToLastParts[0]);
            string secondToLastName = secondToLastParts[1];
            int secondToLastWins = int.Parse(secondToLastParts[2]);
            int secondToLastLosses = int.Parse(secondToLastParts[4].Trim('.')); // Losses is at index 4
            int secondToLastTotalPlayed = int.Parse(secondToLastParts[5].Trim('.'));
            int secondToLastScore = int.Parse(secondToLastParts[6]);
            int[] secondToLastLineData = new int[] { secondToLastPos, secondToLastWins, secondToLastLosses, secondToLastTotalPlayed, secondToLastScore };

            return (secondToLastLineData, lastLineData);
        }

        private static string GetCheckFileName(string squadronName)
        {
            if (squadronName == "BufSs")
            {
                return "CheckBufSs.txt";
            }
            else if (squadronName == "BofSs")
            {
                return "CheckBofSs.txt";
            }
            else if (squadronName == "BriSs")
            {
                return "CheckBriSs.txt";
            }
            else
            {
                throw new ArgumentException("Invalid squadron name.");
            }
        }




        public class ScoreExtractor
        {
            public int ExtractScoreOfBofSs()
            {
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "TopSquadTotals_*.txt");
                if (files.Length == 0)
                {
                    Console.WriteLine("No recent files found for extraction.");
                    int squadronTotalScore = 0;
                    return -1; // Or throw an exception
                }

                Array.Sort(files);
                string mostRecentFile = files[files.Length - 1];

                string[] lines = File.ReadAllLines(mostRecentFile);
                string lineWithBofSs = lines.FirstOrDefault(line => line.Contains("BofSs"));

                if (lineWithBofSs == null)
                {
                    Console.WriteLine("No data found for squadron BofSs.");
                    int squadronTotalScore = 0;
                    return -1; // Or throw an exception
                }

                int index = lineWithBofSs.IndexOf("Score:");
                if (index == -1)
                {
                    Console.WriteLine("Score not found for squadron BofSs.");
                    int squadronTotalScore = 0;
                    return -1; // Or throw an exception
                }

                string scorePart = lineWithBofSs.Substring(index + 7); // 7 is the length of "Score: "
                int score = int.Parse(scorePart.Trim());
                return score;
            }
        }

        private async Task ProcessIndivPointsChange()
        {
            Console.WriteLine("5 minutes elapsed!");
            IMessageChannel chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;

            Commands commands = new Commands();
            SquadronObj oldSqd = await commands.LoadSqd("BofSs");
            await Handle5MinuteWriteTimer("BofSs");
            SquadronObj newSqd = await commands.LoadSqd("BofSs");

            List<Commands.PlayerRatingChange> ratingChanges = commands.CompareSquadrons(oldSqd, newSqd);

            if (oldSqd.Score != newSqd.Score)
            {
                foreach (var change in ratingChanges)
                {
                    if (change.NewRating - change.OldRating > 0)
                    {
                        await chnl.SendMessageAsync($"WIN! {change.PlayerName}, Old: {change.OldRating}, New: {change.NewRating} Diff: {change.NewRating - change.OldRating}");
                    }
                    else if (change.NewRating - change.OldRating < 0)
                    {
                        await chnl.SendMessageAsync($"LOSS! {change.PlayerName}, Old: {change.OldRating}, New: {change.NewRating} Diff: {change.NewRating - change.OldRating}");
                    }
                    else
                    {
                        await chnl.SendMessageAsync($"{change.PlayerName}, Old: {change.OldRating}, New: {change.NewRating} Diff: {change.NewRating - change.OldRating}");
                    }
                }

                await chnl.SendMessageAsync("---------- Done ----------");
            }
        }

        private async Task pointsCheckBriSs()
        {
            
            if (briSsScoreTracking = true)
            {

                //Console.WriteLine("1 minute elapsed!");

                IMessageChannel chnl = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;
                IMessageChannel chnl2 = _client.GetChannel(esperbotchannel) as IMessageChannel;


                //await chnl.SendMessageAsync("triggering pointsCheckBriSs");

                Commands commands = new Commands();
                SquadronObj oldSqd = await commands.LoadSqd("BriSs");
                await Handle5MinuteWriteTimer("BriSs");
                SquadronObj newSqd = await commands.LoadSqd("BriSs");

                List<Commands.PlayerRatingChange> ratingChanges = commands.CompareSquadrons(oldSqd, newSqd);

                if (oldSqd.Score != newSqd.Score)
                {
                    //await chnl.SendMessageAsync("old score != new score");
                    foreach (var change in ratingChanges)
                    {
                        if (change.NewRating - change.OldRating > 0)
                        {
                            await chnl.SendMessageAsync($"WIN! {change.PlayerName}, Old: {change.OldRating}, New: {change.NewRating} Diff: {change.NewRating - change.OldRating}");
                        }
                        else if (change.NewRating - change.OldRating < 0)
                        {
                            await chnl.SendMessageAsync($"LOSS! {change.PlayerName}, Old: {change.OldRating}, New: {change.NewRating} Diff: {change.NewRating - change.OldRating}");
                        }
                        else
                        {
                            await chnl.SendMessageAsync($"{change.PlayerName}, Old: {change.OldRating}, New: {change.NewRating} Diff: {change.NewRating - change.OldRating}");
                        }
                    }

                    await chnl.SendMessageAsync("---------- Done ----------");
                    await chnl2.SendMessageAsync("BriSs had a points change.  Beta feature, see testing channel.");
                }
            }
        }

        private async Task grabCheck(string squadron)
        {
            SquadronObj sqd = await Webscraper.ScrapeCheck($"!check {squadron}");

        }
        private async Task HandleAltVehiclesCommand(SocketMessage message)
        {
            string csvFilePath = "AltSpreadsheet.csv"; // Replace with the actual path
            string userInput = message.Content; // Get the user's message

            string[] parts = userInput.Split(' ');

            if (parts.Length != 2 || !parts[0].Equals("!altvehicles", StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Invalid input. Use '!altvehicles <alt name>'.");
                return;
            }

            string altName = parts[1];

            altName = SanitizeInput(altName);

            if (string.IsNullOrEmpty(altName))
            {
                await message.Channel.SendMessageAsync("Invalid alt name.");
                return;
            }

            Dictionary<string, List<(string Header, string Vehicle)>> altVehicles = GetAltVehicles(csvFilePath);

            if (altVehicles == null)
            {
                await message.Channel.SendMessageAsync("Error reading CSV file.");
                return;
            }

            if (altVehicles.ContainsKey(altName))
            {
                string output = $"Vehicles for {altName}:\n";
                foreach (var vehicleData in altVehicles[altName])
                {
                    output += $"- {vehicleData.Header}: {vehicleData.Vehicle}\n";
                }
                await message.Channel.SendMessageAsync(output);

            }
            else
            {
                await message.Channel.SendMessageAsync($"Alt name '{altName}' not found.");
            }
        }

        [CommandDescription("Gives you a number to help with queue ordering")]
        private async Task HandleTakeANumberCommand(SocketMessage message)
        {
            var user = message.Author as SocketGuildUser;
            var username = user?.Nickname ?? user?.Username ?? "Unknown User";
            var userId = user.Id;

            // Assign a new number (overwrite old one if exists)
            userNumbers[userId] = takeaANumberNumber;
            await message.Channel.SendMessageAsync($"Okay, {username}, you are now number {takeaANumberNumber}.");
            takeaANumberNumber++;

            HandleShowNumbersCommand(message);
        }

        // Command to display all assigned numbers
        [CommandDescription("Shows who has what number.")]
        private async Task HandleShowNumbersCommand(SocketMessage message)
        {
            if (userNumbers.Count == 0)
            {
                await message.Channel.SendMessageAsync("No numbers have been assigned yet.");
                return;
            }

            var response = "**Current number assignments:**\n";
            foreach (var entry in userNumbers)
            {
                var user = (message.Channel as SocketGuildChannel)?.Guild.GetUser(entry.Key);
                var username = user?.Nickname ?? user?.Username ?? "Unknown User";
                response += $"{entry.Value}: {username}\n";
            }

            await message.Channel.SendMessageAsync(response);
        }

        public static Dictionary<string, List<(string Header, string Vehicle)>> GetAltVehicles(string csvFilePath)
        {
            Dictionary<string, List<(string Header, string Vehicle)>> altVehicles = new Dictionary<string, List<(string Header, string Vehicle)>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var reader = new StreamReader(csvFilePath))
                {
                    string headerLine = reader.ReadLine(); // Read the header line
                    string[] headers = headerLine.Split(',');

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = line.Split(',');

                        if (fields.Length > 2) // Check to avoid index out of bounds exception.
                        {
                            string playerName = fields[2].Trim('"'); // Player name is in the THIRD column (index 2). Trim quotes

                            if (string.IsNullOrEmpty(playerName)) continue; // Skip empty rows

                            List<(string Header, string Vehicle)> vehicles = new List<(string Header, string Vehicle)>();

                            for (int i = 8; i <= 53 && i < fields.Length && i < headers.Length; i++) //Columns I to BI (8 to 53)
                            {
                                string vehicle = fields[i].Trim('"'); // Trim quotes
                                string header = headers[i].Trim('"'); // Get header for the column

                                if (!string.IsNullOrEmpty(vehicle))
                                {
                                    vehicles.Add((header, vehicle));
                                }
                            }

                            altVehicles[playerName] = vehicles;
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: CSV file not found at {csvFilePath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV: {ex.Message}");
                return null;
            }

            return altVehicles;
        }

        [CommandDescription("!turn <setting> on|off")]
        private async Task HandleTurnCommand(SocketMessage message)
        {
            
            string[] tempParts = message.Content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string[] parts;
            if (tempParts.Length >= 3)
            {
                parts = new string[] { tempParts[0], tempParts[1], string.Join(" ", tempParts.Skip(2)) };
            }
            else
            {
                parts = tempParts;
            }

            if (parts.Length < 3)
            {
                await message.Channel.SendMessageAsync("Usage: !turn <feature> <on/off>");
                return;
            }

            string feature = parts[1].ToLower();
            string action = parts[2].ToLower();

            if (!featureToggles.ContainsKey(feature))
            {
                await message.Channel.SendMessageAsync($"Unknown feature '{feature}'. Available features: {string.Join(", ", featureToggles.Keys)}");
                return;
            }

            if (action != "on" && action != "off")
            {
                await message.Channel.SendMessageAsync("Invalid action. Use 'on' or 'off'.");
                return;
            }

            featureToggles[feature] = (action == "on");
            await message.Channel.SendMessageAsync($"{feature} is now {action.ToUpper()}.");
        }

        [CommandDescription("Displays current Settings")]
        private async Task HandleSettingsCommand(SocketMessage message)
        {
            if (featureToggles.Count == 0)
            {
                await message.Channel.SendMessageAsync("No settings available.");
                return;
            }

            string settings = "Current Settings:\n" +
                              string.Join("\n", featureToggles.Select(kvp => $"- {kvp.Key}: {(kvp.Value ? "ON" : "OFF")}"));

            await message.Channel.SendMessageAsync(settings);
        }

        private async Task SecretdmtestCommand(SocketMessage message)
        {
            if (message.Content.StartsWith("!dmtest"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID

                if (message.Author.Id == yourUserId) //makes sure only you can trigger this command.
                {
                    var user = _client.GetUser(yourUserId);
                    if (user != null)
                    {
                        await user.SendMessageAsync("Hello from your bot!");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Could not find user.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("You are not authorized to use this command.");
                }

            }
        }


        private async Task SecrettestcmdCommand(SocketMessage message)
        {
            if (message.Content.StartsWith("!testcmd"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID

                if (message.Author.Id == yourUserId) // Makes sure only you can trigger this command.
                {
                    if (message.Channel is SocketGuildChannel guildChannel) // Ensure the message is from a guild channel
                    {
                        SocketGuild guild = guildChannel.Guild; // Get the guild

                        var user = _client.GetUser(yourUserId);
                        if (user != null)
                        {
                            StringBuilder channelList = new StringBuilder();
                            channelList.AppendLine("Channels in this server:");

                            foreach (SocketGuildChannel channel in guild.Channels)
                            {
                                channelList.AppendLine($"- {channel.Name} (ID: {channel.Id})");
                            }

                            await SendLongMessage(user, channelList.ToString()); // Use the SendLongMessage method
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("Could not find user.");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("This command can only be used in a server channel.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("You are not authorized to use this command."); // Generic error message
                }
            }
        }


        private async Task Secret2testcmdCommand(SocketMessage message)
        {
            if (message.Content.StartsWith("!2testcmd"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID
                ulong targetChannelId = 1173393377929203822; // Replace with your target channel ID
                ulong firstMessageId = 1349559895741038613; // Replace with the ID you copied

                if (message.Author.Id == yourUserId)
                {
                    var targetChannel = _client.GetChannel(targetChannelId) as SocketTextChannel;

                    if (targetChannel != null)
                    {
                        string[] parts = message.Content.Split(' ');
                        ulong fromMessageId = firstMessageId; // Always start from the first message

                        if (parts.Length > 1 && ulong.TryParse(parts[1], out fromMessageId))
                        {
                            // Start from a specific message ID
                        }

                        IEnumerable<IMessage> messages;

                        messages = await targetChannel.GetMessagesAsync(fromMessageId, Direction.After, limit: 50).FlattenAsync();

                        if (messages.Any())
                        {
                            StringBuilder history = new StringBuilder();
                            history.AppendLine($"Message history for {targetChannel.Name} (Starting from: {fromMessageId}):");

                            foreach (IMessage msg in messages)
                            {
                                history.AppendLine($"- {msg.Author.Username}: {msg.Content} ({msg.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}) (ID: {msg.Id})");
                            }

                            await SendLongMessage(message.Author, history.ToString());
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("No more messages found.");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Target channel not found.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("You are not authorized to use this command.");
                }
            }
        }


        private async Task Secret3testcmdCommand(SocketMessage message)
        {
            if (message.Content.StartsWith("!3testcmd"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID
                ulong targetCategoryId = 1173393243883442236; // Replace with your target category ID

                if (message.Author.Id == yourUserId)
                {
                    if (message.Channel is SocketGuildChannel guildChannel)
                    {
                        SocketGuild guild = guildChannel.Guild;

                        var category = guild.GetCategoryChannel(targetCategoryId);

                        if (category != null)
                        {
                            var channelsInCategory = category.Channels;

                            if (channelsInCategory.Any())
                            {
                                StringBuilder channelList = new StringBuilder();
                                channelList.AppendLine($"Channels in category '{category.Name}':");

                                foreach (SocketGuildChannel channel in channelsInCategory)
                                {
                                    channelList.AppendLine($"- {channel.Name} (ID: {channel.Id})");
                                }

                                await SendLongMessage(message.Author, channelList.ToString());
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("No channels found in that category.");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("Category not found.");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("This command can only be used in a server channel.");
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("You are not authorized to use this command.");
                }
            }
        }


        private async Task Secret4testcmdCommand(SocketMessage message)
        {
            if (message.Content.StartsWith("!4testcmd"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID
                ulong targetChannelId = 1173393377929203822; // Replace with your target channel ID

                if (message.Author.Id == yourUserId)
                {
                    var targetChannel = _client.GetChannel(targetChannelId) as SocketTextChannel;

                    if (targetChannel != null)
                    {
                        var firstMessage = (await targetChannel.GetMessagesAsync(limit: 1).FlattenAsync()).FirstOrDefault();

                        if (firstMessage != null)
                        {
                            await message.Author.SendMessageAsync($"The ID of the first message is: {firstMessage.Id}");
                        }
                        else
                        {
                            await message.Author.SendMessageAsync("Channel is empty.");
                        }
                    }
                    else
                    {
                        await message.Author.SendMessageAsync("Target channel not found.");
                    }
                }
                else
                {
                    await message.Author.SendMessageAsync("You are not authorized to use this command.");
                }
            }
        }

        private async Task Secret5testcmdCommand(SocketMessage message)
        {
            if (message.Content.StartsWith("!5testcmd"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID
                ulong targetChannelId = 1173393377929203822; // Replace with your target channel ID

                if (message.Author.Id == yourUserId)
                {
                    var targetChannel = _client.GetChannel(targetChannelId) as SocketTextChannel;

                    if (targetChannel != null)
                    {
                        try
                        {
                            // Fetch all messages (up to a reasonable limit, e.g., 1000)
                            var allMessages = await targetChannel.GetMessagesAsync(limit: 1000).FlattenAsync();

                            if (allMessages.Any())
                            {
                                // Reverse the order of messages
                                var reversedMessages = allMessages.Reverse();

                                // Write to a text file
                                string filePath = $"channel_{targetChannelId}_messages.txt";
                                using (StreamWriter writer = new StreamWriter(filePath))
                                {
                                    foreach (IMessage msg in reversedMessages)
                                    {
                                        writer.WriteLine($"- {msg.Author.Username}: {msg.Content} ({msg.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}) (ID: {msg.Id})");
                                    }
                                }

                                await message.Author.SendMessageAsync($"Messages written to {filePath}");
                            }
                            else
                            {
                                await message.Author.SendMessageAsync("No messages found in the channel.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            await message.Author.SendMessageAsync($"An error occurred: {ex.Message}");
                        }
                    }
                    else
                    {
                        await message.Author.SendMessageAsync("Target channel not found.");
                    }
                }
                else
                {
                    await message.Author.SendMessageAsync("You are not authorized to use this command.");
                }
            }
        }


        private async Task SecretRecent50Command(SocketMessage message)
        {
            if (message.Content.StartsWith("!recent50"))
            {
                ulong yourUserId = 308128406699245568; // Replace with your actual user ID
                ulong targetChannelId = 1173393377929203822; // Replace with your target channel ID

                if (message.Author.Id == yourUserId)
                {
                    var targetChannel = _client.GetChannel(targetChannelId) as SocketTextChannel;

                    if (targetChannel != null)
                    {
                        try
                        {
                            var recentMessages = await targetChannel.GetMessagesAsync(limit: 50).FlattenAsync();

                            if (recentMessages.Any())
                            {
                                StringBuilder history = new StringBuilder();
                                history.AppendLine($"Recent 50 Messages in {targetChannel.Name}:");

                                foreach (IMessage msg in recentMessages.Reverse()) // Reverse to display newest first
                                {
                                    history.AppendLine($"- {msg.Author.Username}: {msg.Content} ({msg.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")})");
                                }

                                await SendLongMessage(message.Author, history.ToString());
                            }
                            else
                            {
                                await message.Author.SendMessageAsync($"No messages found in {targetChannel.Name}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            await message.Author.SendMessageAsync($"An error occurred: {ex.Message}");
                        }
                    }
                    else
                    {
                        await message.Author.SendMessageAsync("Target channel not found.");
                    }
                }
                else
                {
                    await message.Author.SendMessageAsync("You are not authorized to use this command.");
                }
            }
        }


        private async Task SendLongMessage(IUser user, string message) // Corrected parameter type
        {
            if (message.Length <= 2000)
            {
                await user.SendMessageAsync(message); // Send DM to user
                return;
            }

            int chunkSize = 2000;
            for (int i = 0; i < message.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, message.Length - i);
                string chunk = message.Substring(i, length);
                await user.SendMessageAsync(chunk); // Send DM to user
            }
        }



        private static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_\s]", ""); // Added underscore
            return sanitized.Trim();
        }

        private async Task ProcessSquadron1mScoreChanges()
        {
            IMessageChannel chnl = _client.GetChannel(esperbotchannel) as IMessageChannel;
            IMessageChannel chnl2 = _client.GetChannel(EsperBotTestingChannel) as IMessageChannel;

            string[] squadrons = { "BofSs", "BufSs", "BriSs" };

            bool anyChanges = false;

            foreach (var squadron in squadrons)
            {
                SquadronObj squadron5m = await Webscraper.ScrapeCheck($"!check {squadron}");

                int startWinsTemp = 0;
                int startLossesTemp = 0;
                int midWinsTemp = 0;
                int midLossesTemp = 0;
                int startPointsTemp = 0;
                int lastWinsTemp = 0;
                int lastLossesTemp = 0;
                int winsDifference = 0;
                int lossesDifference = 0;

                SquadronData data = null; // Declare data here

                if (squadron == "BofSs")
                {
                    data = bofssData; // Assign the correct instance
                    startWinsTemp = startOfSessionWins;
                    startLossesTemp = startOfSessionLosses;
                    midWinsTemp = midSessionWinsCounter;
                    midLossesTemp = midSessionLossesCounter;
                    startPointsTemp = startOfSessionPoints;
                    lastWinsTemp = lastRunsWinsCumulativeCounter;
                    lastLossesTemp = lastRunsLossesCumulativeCounter;
                }
                else if (squadron == "BufSs")
                {
                    data = bufssData; // Assign the correct instance
                    startWinsTemp = StartOfSessionWinsBufSs;
                    startLossesTemp = StartOfSessionLossesBufSs;
                    midWinsTemp = midSessionWinsCounterBufSs;
                    midLossesTemp = midSessionLossesCounterBufSs;
                    startPointsTemp = startOfSessionPointsBufSs;
                    lastWinsTemp = lastRunsWinsCumulativeCounterBufSs;
                    lastLossesTemp = lastRunsLossesCumulativeCounterBufSs;
                }
                else if (squadron == "BriSs")
                {
                    data = brissData;
                    startWinsTemp = StartOfSessionWinsBriSs;
                    startLossesTemp = StartOfSessionLossesBriSs;
                    midWinsTemp = midSessionWinsCounterBriSs;
                    midLossesTemp = midSessionLossesCounterBriSs;
                    startPointsTemp = startOfSessionPointsBriSs;
                    lastWinsTemp = lastRunsWinsCumulativeCounterBriSs;
                    lastLossesTemp = lastRunsLossesCumulativeCounterBriSs;
                }

                // *** KEY CHANGE: Check if the squadron has EVER been initialized ***
                if (!data.HasBeenInitialized)
                {
                    data.Initialize(squadron5m);
                    if (squadron == "BofSs")
                    {
                        startOfSessionWins = data.StartWins;
                        startOfSessionLosses = data.StartLosses;
                        startOfSessionPoints = data.StartPoints;
                    }
                    else if (squadron == "BufSs")
                    {
                        StartOfSessionWinsBufSs = data.StartWins;
                        StartOfSessionLossesBufSs = data.StartLosses;
                        startOfSessionPointsBufSs = data.StartPoints;
                    }            
                    else if (squadron == "BriSs")
                    {
                        StartOfSessionWinsBriSs = data.StartWins;
                        StartOfSessionLossesBriSs = data.StartLosses;
                        startOfSessionPointsBriSs = data.StartPoints;
                    }


                    await chnl2.SendMessageAsync($"Initialized start of session for {squadron}: {data.StartWins} wins, {data.StartLosses} losses, {data.StartPoints} total score.");
                    continue; // Only continue if it's the FIRST ever initialization
                }

                winsDifference = squadron5m.Wins - startWinsTemp - lastWinsTemp;
                lossesDifference = squadron5m.Losses - startLossesTemp - lastLossesTemp;

                if (winsDifference != 0 || lossesDifference != 0)
                {
                    anyChanges = true;

                    if (squadron == "BofSs")
                    {
                        midSessionWinsCounter += winsDifference;
                        midSessionLossesCounter += lossesDifference;
                        lastRunsWinsCumulativeCounter += winsDifference;
                        lastRunsLossesCumulativeCounter += lossesDifference;
                        sessionScoreDelta = squadron5m.Score - startPointsTemp;
                        
                    }
                    else if (squadron == "BufSs")
                    {
                        midSessionWinsCounterBufSs += winsDifference;
                        midSessionLossesCounterBufSs += lossesDifference;
                        lastRunsWinsCumulativeCounterBufSs += winsDifference;
                        lastRunsLossesCumulativeCounterBufSs += lossesDifference;
                        sessionScoreDeltaBufSs = squadron5m.Score - startPointsTemp;
                        
                    }
                    else if (squadron == "BriSs")
                    {
                        midSessionWinsCounterBriSs += winsDifference;
                        midSessionLossesCounterBriSs += lossesDifference;
                        lastRunsWinsCumulativeCounterBriSs += winsDifference;
                        lastRunsLossesCumulativeCounterBriSs += lossesDifference;
                        sessionScoreDeltaBriSs = squadron5m.Score - startPointsTemp;
                        
                    }
                }
               
            }

            if (anyChanges)
            {
                await chnl.SendMessageAsync($"```BofSs: {midSessionWinsCounter}-{midSessionLossesCounter} (Delta: {sessionScoreDelta}). " +
                    $"BufSs is {midSessionWinsCounterBufSs}-{midSessionLossesCounterBufSs} (Delta: {sessionScoreDeltaBufSs})." 
                                         +
                    $" BriSs is {midSessionWinsCounterBriSs}-{midSessionLossesCounterBriSs} (Delta: {sessionScoreDeltaBriSs}).```");






            }
        }


        public class SquadronData
        {
            public int StartWins { get; set; }
            public int StartLosses { get; set; }
            public int MidWins { get; set; }
            public int MidLosses { get; set; }
            public int StartPoints { get; set; }
            public int LastRunsWinsCumulative { get; set; }
            public int LastRunsLossesCumulative { get; set; }
            public int SessionScoreDelta { get; set; }
            public bool HasBeenInitialized { get; set; } = false; // The new flag

            public void Initialize(SquadronObj squadron)
            {
                StartWins = squadron.Wins;
                StartLosses = squadron.Losses;
                MidWins = 0;
                MidLosses = 0;
                StartPoints = squadron.Score;
                LastRunsWinsCumulative = 0;
                LastRunsLossesCumulative = 0;
                SessionScoreDelta = 0;
                HasBeenInitialized = true; // Set the flag after initialization
            }
        }

        //Declare these outside of the method.
        private SquadronData bofssData = new SquadronData();
        private SquadronData bufssData = new SquadronData();
        private SquadronData brissData = new SquadronData();



        private void ResetWLSessionVariables()
        {
            // Reset BofSs variables
            startOfSessionWins = 0;
            startOfSessionLosses = 0;
            startOfSessionPoints = 0;
            midSessionWinsCounter = 0;
            midSessionLossesCounter = 0;
            lastRunsWinsCumulativeCounter = 0;
            lastRunsLossesCumulativeCounter = 0;
            sessionScoreDelta = 0;

            // Reset BufSs variables
            StartOfSessionWinsBufSs = 0;
            StartOfSessionLossesBufSs = 0;
            startOfSessionPointsBufSs = 0;
            midSessionWinsCounterBufSs = 0;
            midSessionLossesCounterBufSs = 0;
            lastRunsWinsCumulativeCounterBufSs = 0;
            lastRunsLossesCumulativeCounterBufSs = 0;
            sessionScoreDeltaBufSs = 0;

            // Reset BriSs variables
            StartOfSessionWinsBriSs = 0;
            StartOfSessionLossesBriSs = 0;
            startOfSessionPointsBriSs = 0;
            midSessionWinsCounterBriSs = 0;
            midSessionLossesCounterBriSs = 0;
            lastRunsWinsCumulativeCounterBriSs = 0;
            lastRunsLossesCumulativeCounterBriSs = 0;
            sessionScoreDeltaBriSs = 0;

            bofssData.HasBeenInitialized = false;
            bufssData.HasBeenInitialized = false;
            brissData.HasBeenInitialized = false;

        }







    }

}
