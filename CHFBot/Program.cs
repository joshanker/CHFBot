using Discord;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using BotCommands;
using System.Threading;
using System.Timers;
using SquadronObjects;




namespace CHFBot
{
    class Program
    {
        private DiscordSocketClient _client;
        public ulong generalChannel = 342132137064923136;
        public ulong testingChannel = 1125693277295886357;
        readonly string token = File.ReadAllText(@"C:\token.txt");
        
        static void Main(string[] args)
            

            => new Program().RunBotAsync().GetAwaiter().GetResult();



        public async Task RunBotAsync()
        {
            
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All 
               
            });

        
            ClearMessageCache();
            _client.Log += Log;

            System.Timers.Timer timer = new System.Timers.Timer();
            timer = new System.Timers.Timer(10000 * 6 * 60); //one hour in milliseconds
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Start();
            Console.WriteLine("Timer is starting!");

            await _client.LoginAsync(TokenType.Bot, token);

            _client.MessageReceived += HandleCommandAsync;

            await _client.StartAsync();
            await Task.Delay(-1);
                   

            async void OnTimedEvent(object source, ElapsedEventArgs e)
            {
                //Do the stuff you want to be done every hour;
                Commands quote = new Commands();

               // await quote.sendQuote(_client);
            }
            
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
        
        private async Task HandleCommandAsync(SocketMessage message)
        {
           // Console.WriteLine("handleCommandAsync has triggered:");
           // Console.WriteLine("message is: .." + message.Content + "..1");

            if (message.Author.IsBot) return; // Ignore messages from other bots

            if (message.Channel.Name == "chf-bot-testing" || message.Channel.Name == "general")
            {
               // Console.WriteLine("In the correct channel: " + message.Channel.Name);

                // Check if the message content is empty or contains only whitespaces
                if (string.IsNullOrWhiteSpace(message.Content))
                {
                    Console.WriteLine("Empty or whitespace message content. Ignoring...");
                    return;
                }

                // Trim any leading or trailing whitespaces from the content
                string content = message.Content.Trim();

                // Debug statement to check the trimmed message content
                //Console.WriteLine("Trimmed message content: " + content);

                if (content.StartsWith("!hello"))
                {
                    Console.WriteLine("!hello detected.");
                    await message.Channel.SendMessageAsync("Well hi there.");
                }
                else if (content.StartsWith("!ping"))
                {
                    Console.WriteLine("!ping detected.");
                    await message.Channel.SendMessageAsync("Pong!");
                }
                else if (content.StartsWith("!join"))
                {
                    Console.WriteLine("!join detected.");
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


                else if (content.StartsWith("!acad"))
                {
                    Commands scrapeAllAndPopulate = new Commands();
                    SquadronObj AcadObj = new SquadronObj("Academy", "https://warthunder.com/en/community/claninfo/The%20Academy");

                    AcadObj = await scrapeAllAndPopulate.scrapeAllAndPopulate(AcadObj).ConfigureAwait(true);

                    await message.Channel.SendMessageAsync("Squadron  Name: " + AcadObj.SquadronName + ". URL: " + AcadObj.sqdurl).ConfigureAwait(true);

                    //scrapeAllAndPopulate.PrintSquadronInfoToDiscord(AcadObj, testingChannel);
                    var chnl = _client.GetChannel(testingChannel) as IMessageChannel; // 4

                    await chnl.SendMessageAsync("Squadron: " + AcadObj.SquadronName);
                    await chnl.SendMessageAsync("Player Count: " + AcadObj.Players.Count);
                    await chnl.SendMessageAsync("-");

                    foreach (Player player in AcadObj.Players)
                    {

                        await chnl.SendMessageAsync("Name: " + player.PlayerName + " \nNumber: " + player.Number + " \nPersonal Clan Rating: " + player.PersonalClanRating + " \nActivity: " + player.Activity + " \nRole: " + player.Rank + " \nDate of Entry: " + player.DateOfEntry + "\n-");

                        //await chnl.SendMessageAsync();
                        //await chnl.SendMessageAsync();
                        //await chnl.SendMessageAsync();
                        //await chnl.SendMessageAsync();
                        //await chnl.SendMessageAsync();
                        //await chnl.SendMessageAsync("-");

                    }

                    await message.Channel.SendMessageAsync("End of squadron printout.").ConfigureAwait(true);

                }

                else if (content.StartsWith("!quote"))
                {
                    Console.WriteLine("!quote called for by "+ message.Author + ".");
                    Commands getQuote = new Commands();
                    string quote = getQuote.getQuote();
                    await message.Channel.SendMessageAsync(quote);
                }

                else
                {
                    Console.WriteLine("No matching command detected.");
                    //await message.Channel.SendMessageAsync("This is the else triggering.");
                    
                }
            }
        }



    }
}