using Discord;
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

            //pointsCheckBriSs();

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

            //pointsCheckBriSs();

        }
        private async void OnOneMinuteEvent(object source, ElapsedEventArgs e)
        {

            if (wlCounter == true)
            {
                //ProcessSquadron5mScoreChange("BofSs");
                //ProcessSquadron5mScoreChange("BufSs");

               ProcessSquadron1mScoreChanges();
                              

            }

            //pointsCheckBriSs();


        }
        private async Task executeTimer(String prefix)
        {
            Commands commands = new Commands();

            SquadronObj sqdObj = new SquadronObj
            {
                url = "https://warthunder.com/en/community/claninfo/Band%20Of%20Scrubs",
                SquadronName = "BofSs"
            };

            await commands.populateScore(sqdObj);
            squadronTotalScore = sqdObj.Score;

            SquadronObj sqdObjBufSs = new SquadronObj
            {
                url = "https://warthunder.com/en/community/claninfo/Bunch%20of%20Scrubs",
                SquadronName = "BufSs"
            };

            await commands.populateScore(sqdObjBufSs);
            squadronTotalScoreBufSs = sqdObjBufSs.Score;

            SquadronObj sqdObjBriSs = new SquadronObj
            {
                url = "https://warthunder.com/en/community/claninfo/Brigade%20of%20Scrubs",
                SquadronName = "BriSs"
            };

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

            HandleCheckCommand("!check BofSs", chnl);
            HandleCheckCommand("!check BufSs", chnl);
            HandleCheckCommand("!check BriSs", chnl);

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
                //else if (content.StartsWith("!join"))
                //{
                //    await HandleJoinCommand(message);
                //}
                // else if (content.StartsWith("!scrapesquadron "))
                //{
                //    await HandleScrapeSquadronCommand(message);
                //}
                //else if (content.StartsWith("!squadronsum "))
                //{
                //    await HandleSquadronSumCommand(message);
                //}
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
                else if (content.StartsWith("!qpoints"))
                {
                    await HandleQpointsCommand(message);
                }
                else if (content.StartsWith("!help"))
                {
                    await HandleCommandsCommand(message);
                }
                else if (content.StartsWith("!trackvoiceupdates"))
                {
                    await HandleTrackVoiceUpdatesCommand(message);
                }
                else if (content.StartsWith("!turnquotes"))
                {
                    await HandleTurnQuotesCommand(message);
                }
                else if (content.StartsWith("!turn5mtimer"))
                {
                    await HandleTurn5mTimerCommand(message);
                }
                else if (content.StartsWith("!turnbundsbotscoretracking"))
                {
                    await HandleTurnBundsBotScoreTrackingCommand(message);
                }
                else if (content.StartsWith("!turnbrissscoretracking"))
                {
                    await HandleTurnBriSsScoreTrackingCommand(message);
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
                else if (content.StartsWith("!2listplayers"))
                {
                    await Handle2ListplayersCommand(message);
                }
                else if (content.StartsWith("!3listplayers"))
                {
                    await Handle3ListplayersCommand(message);
                }
                else if (content.StartsWith("!lastten"))
                {
                    await HandleLastTenCommand(message);
                }
                else if (content.StartsWith("!setwinloss"))
                {
                    await HandleSetWinLossCommand(message);
                }
                else if (content.StartsWith("!2setwinloss"))
                {
                    await Handle2SetWinLossCommand(message);
                }
                else if (content.StartsWith("!setstartingscore"))
                {
                    await HandleSetStartingScoreCommand(message);
                }
                else if (content.StartsWith("!2setstartingscore"))
                {
                    await Handle2SetStartingScoreCommand(message);
                }
                else if (content.StartsWith("!listalts"))
                {
                    await HandleListAltsCommand(message);
                }
                else if (content.StartsWith("!resetsessionwlvariables"))
                {
                    await HandleresetsessionwlvariablesCommand(message);
                }
                //else if (content.StartsWith("!testscrape"))
                //{
                //    await HandleTestScrapeCommand(message);
                //}
                else if (content.StartsWith("!scrape"))
                {
                    await HandleScrapeCommand(message);
                }
                //else if (content.StartsWith("!comparescrape"))
                //{
                //    await HandleCompareScrapeCommand(message);
                //}
                //else if (content.StartsWith("!2comparescrape"))
                //{
                //    await Handle2CompareScrapeCommand(message);
                //}
                else if (content.StartsWith("!comparescrape"))
                {
                    await HandleCompareScrapeCommand(message.Channel);
                }
                else if (content.StartsWith("!squadrontotalscore"))
                {
                    await HandleSquadronTotalScoreCommand(message);
                }
                else if (content.StartsWith("!2squadrontotalscore"))
                {
                    await Handle2SquadronTotalScoreCommand(message);
                }
                else if (content.StartsWith("!check"))
                {
                    await HandleCheckCommand(message);
                }
                else if (content.StartsWith("!altvehicles"))
                {
                    await HandleAltVehiclesCommand(message);
                }
                else if (content.StartsWith("!executetimer"))
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

        [CommandDescription("Gives player count, totals score, and each players' score. (!totals BofSs.")]
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

        [CommandDescription("Scrapes a squadron and write the info to a file.")]
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

        [CommandDescription("Lists the top 20 players in any of the top 10 squadrons and how many points they have.")]
        private async Task HandleTop20Command(SocketMessage message)
        {
            string content = message.Content.Trim();
            string input = content.Substring("!top20 ".Length);

            if (new[] { "Cadet", "BofSs", "Academy", "BufSs", "Early", "RO6", "AVR", "ILWI", "iNut", "SKAL", "NEURO", "LEDAC", "WeBak", "TFedz", "B0AR", "SOFUA", "AFI", "TEHb", "IRAN","BriSs" }.Contains(input))
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

        [CommandDescription("Examines the last two written files for BofSs and lists joiners & leavers. !compare BofSs")]
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

        [CommandDescription("Picks a gamemode, type, and BR.  If you don't like what it chooses, just spam it until you get one you like.")]
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

        [CommandDescription("This is a list of commands you can use. Some need modifiers, like a squadron.")]
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

        [CommandDescription("Who is online and how many points do they have? If player not found, give player name & Discord ID to Esper.")]
        private async Task HandleQpointsCommand(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few seconds.");

            Commands commands = new Commands();
            //Commands scrapeAllAndPopulate = new Commands();
            SquadronObj squadronObject = new SquadronObj();
            squadronObject = commands.validateSquadron("BofSs");


            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);

            //700529928948678777 (Lounge)
            //1133615880488628344 (esper bot testing)
            //200594110044700675 (server ID)

            // var chnl = message.Channel as IMessageChannel; // 4

            ulong channelId = (ulong)700529928948678777;
            IVoiceChannel voiceChannel = _client.GetChannel(channelId) as IVoiceChannel;

            // Create a list to store member usernames
            List<string> playerList = new List<string>();
            var playerInfoList = new List<(string PlayerName, string Points)>();

            await Task.Delay(1000);
            playerList = await commands.GeneratePlayerList(_client, voiceChannel.Id, playerList);

            string[] itemsToJoin = playerList.Take(playerList.Count - 1).ToArray();
            string playerListString = string.Join("", itemsToJoin).ToString();

            await Task.Delay(1000);
            await commands.UpdatePlayerIDs(squadronObject);
            await Task.Delay(1000);

            StringBuilder responseBuilder = new StringBuilder();

            foreach (var playerName in playerList)
            {
                if (ulong.TryParse(playerName.Split(' ')[0], out ulong discordId))
                {
                    Player player = squadronObject.Players.FirstOrDefault(p => p.DiscordID == discordId);

                    if (player != null)
                    {
                        playerInfoList.Add((player.PlayerName, player.PersonalClanRating.ToString()));
                    }
                    else
                    {
                        ulong userId = discordId;
                        var user = _client.GetUser(userId);

                        if (user != null)
                        {
                            playerInfoList.Add((discordId.ToString(), $"Player not found - {user.Username}"));
                        }
                    }
                }
                else
                {
                    playerInfoList.Add((playerName, "Invalid format"));
                }
            }

            // Send the response as a message

            //await message.Channel.SendMessageAsync(responseBuilder.ToString());
            // Sort the list by points in descending order
            //playerInfoList = playerInfoList.OrderByDescending(p => int.Parse(p.Points)).ToList();
            playerInfoList = playerInfoList.OrderByDescending(p =>
            {
                if (int.TryParse(p.Points, out int points))
                {
                    return points;
                }
                return 0; // Set a default value for invalid points
            }).ToList();




            //var responseBuilder = new StringBuilder();

            // Iterate through the sorted list and build your response
            foreach (var playerInfo in playerInfoList)
            {
                responseBuilder.AppendLine($"{playerInfo.PlayerName,-20}: {playerInfo.Points,-6} points");
            }




            await commands.SendLongContentAsEmbedAsync(message.Channel, responseBuilder.ToString()); //Player Names and Points

            await commands.SendLongContentAsEmbedAsync(message.Channel, playerListString); //IDs and Discord Names


            //await message.Channel.SendMessageAsync(response);
        }

        private async Task HandleTop20NoArgCommand(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("I need a squadron, too.  You can enter \"Cadet\", \"BofSs\", \"Academy\", \"Early\", \"RO6\", \"AVR\", \"ILWI\", \"iNut\", \"SKAL\", \"NEURO\", \"LEDAC\", \"B0AR\", \"SOFUA\", \"TFedz\",\"AFI\",\"TEHb\",\"IRAN\",\"BriSs\" This is case-sensitive");
        }

        [CommandDescription("turns on & off login/logoff/move notifs.")]
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

        private async Task HandleresetsessionwlvariablesCommand(SocketMessage message)
            
        {
            if (message.Content == "!resetsessionwlvariables")
            {
                ResetWLSessionVariables();
                await message.Channel.SendMessageAsync("reset");   
            }

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

        private async Task Handle2RecordCommand(SocketMessage message)
        {
            if (message.Content == "!2record")
            {
                //await message.Channel.SendMessageAsync("Win/Loss count for this session is: " + winCounter + "-" + lossCounter + ".");
                SquadronObj squadron5m = await Webscraper.ScrapeCheck($"!check BufSs");

                await message.Channel.SendMessageAsync($"Win/Loss count for this session is: {midSessionWinsCounterBufSs}-{midSessionLossesCounterBufSs} ({squadron5m.Score - startOfSessionPointsBufSs}).");
            }
        }

        //[CommandDescription("turns on & off hourly Quotes.")]
        private async Task HandleTurnQuotesCommand(SocketMessage message)
        {
            if (message.Content == "!turnquotes on")
            {
                quotes = true;
                await message.Channel.SendMessageAsync("OK, turning on hourly quotes.");
            }

            else if (message.Content == "!turnquotes off")
            {
                quotes = false;
                await message.Channel.SendMessageAsync("OK, turning off hourly quotes.");
            }
            else
            {
                await message.Channel.SendMessageAsync("Sorry, the only options are \"on\" and \"off\".  \nThe current status of quotes is: " + quotes.ToString());

            }
        }

        //[CommandDescription("turns on & off the 5 minute timer.")]
        private async Task HandleTurn5mTimerCommand(SocketMessage message)
        {
            if (message.Content == "!turn5mtimer on")
            {
                minuteTimerFive = true;
                await message.Channel.SendMessageAsync("OK, turning on the 5 minute timer");
            }

            else if (message.Content == "!turn5mtimer off")
            {
                minuteTimerFive = false;
                await message.Channel.SendMessageAsync("OK, turning off the 5 minute timer");
            }
            else
            {
                await message.Channel.SendMessageAsync("Sorry, the only options are \"on\" and \"off\".  \nThe current status of the 5 minute timer is: " + minuteTimerFive.ToString());

            }
        }

        //[CommandDescription("turns on & off BundsBot Score reporting")]
        private async Task HandleTurnBundsBotScoreTrackingCommand(SocketMessage message)
        {
            if (message.Content.ToLower() == "!turnbundsbotscoretracking on")
            {
                bundsBotScoreTracking = true;
                await message.Channel.SendMessageAsync("OK, turning on BundsBot score tracking");
            }

            else if (message.Content.ToLower() == "!turnbundsbotscoretracking off")
            {
                bundsBotScoreTracking = false;
                await message.Channel.SendMessageAsync("OK, turning off BundsBot score tracking");
            }
            else
            {
                await message.Channel.SendMessageAsync("Sorry, the only options are \"on\" and \"off\".  \nThe current status of BundsBot score tracking: " + bundsBotScoreTracking.ToString());

            }
        }

        //[CommandDescription("turns on & off BriSs Score reporting")]
        private async Task HandleTurnBriSsScoreTrackingCommand(SocketMessage message)
        {
            if (message.Content.ToLower() == "!turnbrissscoretracking on")
            {
                briSsScoreTracking = true;
                await message.Channel.SendMessageAsync("OK, turning on BriSs score tracking");
            }

            else if (message.Content.ToLower() == "!turnbrissscoretracking off")
            {
                briSsScoreTracking = false;
                await message.Channel.SendMessageAsync("OK, turning off BriSs score tracking");
            }
            else
            {
                await message.Channel.SendMessageAsync("Sorry, the only options are \"on\" and \"off\".  \nThe current status of BriSs score tracking: " + briSsScoreTracking.ToString());

            }
        }

        [CommandDescription("Listplayers <over> / <under> <points> - example: \"!listplayers under 1500\"")]
        private async Task HandleListplayersCommand(SocketMessage message)
        {
            string content = message.Content.Trim();

            // Split the input string into words
            string[] words = content.Split(' ');

            // Check that the first word is "!listplayers"
            if (words[0] != "!listplayers")
            {
                // If the first word is not "!listplayers", then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            // Check that the second word is either "over" or "under"
            if (words[1] != "over" && words[1] != "under")
            {
                // If the second word is not "over" or "under", then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            // Check that the third word is a valid number
            int points;
            if (!int.TryParse(words[2], out points))
            {
                // If the third word is not a valid number, then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            string overUnder = words[1];

            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

            Commands commands = new Commands();
            SquadronObj squadronObject = new SquadronObj();

            squadronObject = commands.validateSquadron("BofSs");

            var chnl = message.Channel as IMessageChannel;

            chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

            squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


            //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
            //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
            //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

            commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        }

        [CommandDescription("2")]
        private async Task Handle2ListplayersCommand(SocketMessage message)
        {
            string content = message.Content.Trim();

            // Split the input string into words
            string[] words = content.Split(' ');

            // Check that the first word is "!listplayers"
            if (words[0] != "!2listplayers")
            {
                // If the first word is not "!listplayers", then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            // Check that the second word is either "over" or "under"
            if (words[1] != "over" && words[1] != "under")
            {
                // If the second word is not "over" or "under", then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            // Check that the third word is a valid number
            int points;
            if (!int.TryParse(words[2], out points))
            {
                // If the third word is not a valid number, then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            string overUnder = words[1];

            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

            Commands commands = new Commands();
            SquadronObj squadronObject = new SquadronObj();

            squadronObject = commands.validateSquadron("BufSs");



            var chnl = message.Channel as IMessageChannel;

            chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

            squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


            //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
            //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
            //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

            commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        }

        [CommandDescription("3")]
        private async Task Handle3ListplayersCommand(SocketMessage message)
        {
            string content = message.Content.Trim();

            // Split the input string into words
            string[] words = content.Split(' ');

            // Check that the first word is "!listplayers"
            if (words[0] != "!3listplayers")
            {
                // If the first word is not "!listplayers", then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            // Check that the second word is either "over" or "under"
            if (words[1] != "over" && words[1] != "under")
            {
                // If the second word is not "over" or "under", then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            // Check that the third word is a valid number
            int points;
            if (!int.TryParse(words[2], out points))
            {
                // If the third word is not a valid number, then the input is invalid
                await message.Channel.SendMessageAsync("Invalid command. Please use !listplayers <over> / <under> <points>");
                return;
            }

            string overUnder = words[1];

            await message.Channel.SendMessageAsync("Please wait, scraping.... This might take a few moments.");

            Commands commands = new Commands();
            SquadronObj squadronObject = new SquadronObj();

            squadronObject = commands.validateSquadron("BriSs");



            var chnl = message.Channel as IMessageChannel;

            chnl.SendMessageAsync("Players with score " + overUnder + " " + points + ":");

            squadronObject = await commands.populateScore(squadronObject).ConfigureAwait(true);
            squadronObject = await commands.scrapeAllAndPopulate(squadronObject).ConfigureAwait(true);


            //await chnl.SendMessageAsync("Squadron: " + squadronObject.SquadronName);
            //await chnl.SendMessageAsync("Player Count: " + squadronObject.Players.Count);
            //await chnl.SendMessageAsync("Score: " + squadronObject.Score.ToString());

            commands.printPlayersOverUnder(chnl, squadronObject, overUnder, points);

        }

        [CommandDescription("Displays the last ten SRE session counts.")]
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
        private async Task HandleSetWinLossCommand(SocketMessage message)
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

        private async Task Handle2SetWinLossCommand(SocketMessage message)
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

        private async Task HandleSetStartingScoreCommand(SocketMessage message)
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

        private async Task Handle2SetStartingScoreCommand(SocketMessage message)
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

        [CommandDescription("Updates the daily total to the current total.")]
        private async Task HandleSquadronTotalScoreCommand(SocketMessage message)
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
        private async Task Handle2SquadronTotalScoreCommand(SocketMessage message)
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


                    string[] lines = messageBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    // Skip the first line and join the remaining lines
                    string contentToWrite = string.Join(Environment.NewLine, lines.Skip(1));

                    // Write the content to the file
                    //await File.WriteAllTextAsync(filePath, contentToWrite);


                    writer.Write(contentToWrite);
                }

            }
            else
            {
                Console.WriteLine("error in the else of writecheck.");
            }
        }

        public static (int[], int[]) ReadCheck(string squadronName)
        {
            // Determine the file name based on the squadron name
            string fileName;
            if (squadronName == "BufSs")
            {
                fileName = "CheckBufSs.txt";
            }
            else if (squadronName == "BofSs")
            {
                fileName = "CheckBofSs.txt";
            }
            else if (squadronName == "BriSs")
            {
                fileName = "CheckBriSs.txt";
            }
            else
            {
                throw new ArgumentException("Invalid squadron name. Must be either 'BufSs' or 'BofSs' or 'BriSs'.");
            }

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"The file '{fileName}' does not exist.");
            }

            string[] lines = File.ReadAllLines(fileName);
            if (lines.Length < 0)
            {
                throw new InvalidOperationException("The file doesn't contain enough lines.");
            }

            string lastLine = lines.Last();
            string[] parts = lastLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6)
            {
                throw new InvalidOperationException("The last line does not contain enough parts.");
            }
            int pos = int.Parse(parts[0]);
            string name = parts[1];
            int wins = int.Parse(parts[2]);

            parts[4] = parts[4].Trim('.');
            int losses = int.Parse(parts[4]); // Note: Skipping the "&" part

            parts[5] = parts[5].Trim('.');
            int totalPlayed = int.Parse(parts[5]);
            int score = int.Parse(parts[6]);

            int[] lastLine2 = new int[] { pos, wins, losses, totalPlayed, score };


            string secondToLastLine = lines[lines.Length - 2];
            string[] parts2 = secondToLastLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6)
            {
                throw new InvalidOperationException("The last line does not contain enough parts.");
            }
            int pos2 = int.Parse(parts2[0]);
            string name2 = parts2[1];
            int wins2 = int.Parse(parts2[2]);

            parts2[4] = parts2[4].Trim('.');
            int losses2 = int.Parse(parts2[4]); // Note: Skipping the "&" part

            parts2[5] = parts2[5].Trim('.');
            int totalPlayed2 = int.Parse(parts2[5].Trim('.').Trim()); // Remove the trailing dot
            int score2 = int.Parse(parts2[6]);

            int[] secondToLastLine2 = new int[] { pos2, wins2, losses2, totalPlayed2, score2 };



            //int[] secondToLastLine = lines[lines.Length - 2];



            return (secondToLastLine2, lastLine2);
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


                    await chnl.SendMessageAsync($"Initialized start of session for {squadron}: {data.StartWins} wins, {data.StartLosses} losses, {data.StartPoints} total score.");
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
