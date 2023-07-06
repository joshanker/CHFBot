using Discord;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using Scraper;
using BotCommands;
using System.Threading;




namespace CHFBot
{
    class Program
    {
        private DiscordSocketClient _client;

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

            await _client.LoginAsync(TokenType.Bot, "MTEyNTY4NTg4NzE4ODA4Njg2Nw.Gdtnxb.9LzfrwI8CEuCtPgTmDcOcMFgrsM-NDcrDUW3rI");

            _client.MessageReceived += HandleCommandAsync;

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

       

        private async Task HandleCommandAsync(SocketMessage message)
        {
           // Console.WriteLine("handleCommandAsync has triggered:");
           // Console.WriteLine("message is: .." + message.Content + "..1");

            if (message.Author.IsBot) return; // Ignore messages from other bots

            if (message.Channel.Name == "chf-bot-testing")
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
                else if (content.StartsWith("!scrapetitle"))
                {
                    Console.WriteLine("!scraping title.");

                    string url = "https://warthunder.com/en/community/claninfo/Cadet";
                    Webscraper scraper = new Webscraper();
                    string title = await scraper.ScrapeWebsiteTitleAsync(url);
                    Console.WriteLine("Website title: " + title);

                    await message.Channel.SendMessageAsync("The title of that webpage is: " + title);
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
                    await message.Channel.SendMessageAsync("This is the else triggering.");
                    return;
                }
            }
        }



    }
}