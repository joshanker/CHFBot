using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scraper;
using System.Timers;
using Discord;
using Discord.WebSocket;
using SquadronObjects;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;
using System.CodeDom;
using System.Diagnostics.Eventing.Reader;

namespace Scraper
{
    public class Webscraper
    {
        public Webscraper()
        {
            // Constructor logic here
        }

        public async Task<string> ScrapeWebsiteAllAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);
                HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//table[@class='wt-clanlist-table']/tbody/tr");

                for (int i = 8; i < 771; i = i + 6)
                {
                    string xpath = "//*[@id=\"bodyRoot\"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + i + "]/a";

                    HtmlNode n1 = doc.DocumentNode.SelectSingleNode(xpath);
                    String n1t = n1.InnerText;
                    Console.WriteLine(n1t);

                }

                //if (rows != null)
                //{
                //    foreach (HtmlNode row in rows)
                //    {
                //        string name = row.SelectSingleNode(".//td[1]").InnerText.Trim();
                //        string activity = row.SelectSingleNode(".//td[2]").InnerText.Trim();
                //        string role = row.SelectSingleNode(".//td[3]").InnerText.Trim();
                //        string rating = row.SelectSingleNode(".//td[4]").InnerText.Trim();
                //        string dateOfEntry = row.SelectSingleNode(".//td[5]").InnerText.Trim();


                //        Console.WriteLine("Name: " + name);
                //        Console.WriteLine("Activity: " + activity);
                //        Console.WriteLine("Role: " + role);
                //        Console.WriteLine("Rating: " + rating);
                //        Console.WriteLine("Date of Entry: " + dateOfEntry);
                //        Console.WriteLine();

                //        return "I hit the return in the if";
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("No data found.");
                //    return "I hit the return in the else";
                //}

                Console.ReadLine();
                return "blah blah";

            }
        }

        public async Task<SquadronObj> ScrapeWebsiteAllAndPopulateAsync(SquadronObj objname)
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(objname.url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                //HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//table[@class='wt-clanlist-table']/tbody/tr");


                //How many times do we run our loop?  Once for each player, but how to tell how many?
                //here's the xpath for how many players...
                //*[@id="squadronsInfoRoot"]/div[2]/div[2]
                
                string playerCountPath = "//*[@id='squadronsInfoRoot']/div[2]/div[2]";
                HtmlNode howMany = doc.DocumentNode.SelectSingleNode(playerCountPath);
                string value = howMany?.InnerText;
                string digitsOnly = Regex.Replace(value, @"\D", "");
                char[] charsToTrim = { '"' };

                int number = Int32.Parse(digitsOnly);

                //for (int i = 8; i < 25; i = i + 6)
                //for (int i = 8; i < 771; i = i + 6)
                int runNumber = 0;
                for (int i = 8; i < 6 * number + 6; i = i + 6)
                {
                    //Console.WriteLine("i is now " + i);
                    
                    runNumber = runNumber + 1;
                    //Console.WriteLine("this is run number" + runNumber);
                    Player newp = new Player();

                    string namexpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + i + "]/a";

                    //*[@id="bodyRoot"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[134]/noindex/div/a
                    //*[@id="bodyRoot"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[128]/a

                    //*[@id="bodyRoot"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[134]/noindex/div/a



                    HtmlNode node = doc.DocumentNode.SelectSingleNode(namexpath);

                    string suffixToReplace = "/noindex/div/a";
                    string replacement = "/a";
                    if (node == null) {
                        
                        {
                            // Replace the suffix with the desired replacement
                            string result = namexpath.Substring(0, namexpath.Length - suffixToReplace.Length) + replacement;
                            Console.WriteLine("replacing. Result is: " + result);
                            node = doc.DocumentNode.SelectSingleNode(result);
                        }
                        

                    }


                    //Console.WriteLine($"{namexpath}");  
                    String plrname = WebUtility.HtmlDecode(node.InnerHtml.Trim('\n').Trim());

                    // Check if the node's OuterHtml contains the email flag
                    if (node.OuterHtml.Contains("<span class=\"__cf_email__\""))
                    {
                        {
                            // Extract the player nickname from the URL part of OuterHtml
                            int index = node.OuterHtml.IndexOf("nick=");
                            if (index >= 0)
                            {
                                plrname = node.OuterHtml.Substring(index + 5);
                                int endIndex = plrname.IndexOf('"');
                                if (endIndex >= 0)
                                {
                                    plrname = plrname.Substring(0, endIndex);
                                }
                            }
                        }
                    }

                    newp = objname.setName(newp, plrname);

                    int j = i - 1;
                    string numXpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + j + "]";
                    node = doc.DocumentNode.SelectSingleNode(numXpath);
                    int num = Int32.Parse(node.InnerText.Trim());
                    newp = objname.setNumber(newp, num);

                    int k = i + 1;
                    string ratingXpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + k + "]";
                    node = doc.DocumentNode.SelectSingleNode(ratingXpath);
                    string rating = node.InnerText.Trim();
                    newp = objname.setRating(newp, Int32.Parse(rating));

                    int l = i + 2;
                    string ActivityXpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + l + "]/text()";
                    //*[@id="bodyRoot"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[10]/text()
                    node = doc.DocumentNode.SelectSingleNode(ActivityXpath);
                    string activity = node.InnerText.Trim();
                    newp = objname.setActivity(newp, activity);

                    int m = i + 3;
                    string RankXpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + m + "]";
                    node = doc.DocumentNode.SelectSingleNode(RankXpath);
                    //*[@id="bodyRoot"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[11]
                    string rank = node.InnerText.Trim();
                    newp = objname.setRank(newp, rank);

                    int n = i + 4;
                    String DoEXpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + n + "]";
                    node = doc.DocumentNode.SelectSingleNode(DoEXpath);
                    //*[@id="bodyRoot"]/div[4]/div[2]/div[3]/div/section/div[3]/div/div[12]
                    string test = node.InnerText.Trim();
                    DateTime DoE = DateTime.ParseExact(node.InnerText.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    newp = objname.setDoE(newp, DoE);

                    objname.AddPlayerTolist(newp);

                }

                string scorePath = "//*[@id='bodyRoot']/div[4]/div[2]/div[3]/div/section/div[2]/div[3]/div[2]/div[1]/div[2]";

                HtmlNode score = doc.DocumentNode.SelectSingleNode(scorePath);
                string valueScore = score.InnerText;

                objname.Score = Int32.Parse(valueScore);

                return objname;

            }
        }

        public async Task<SquadronObj> scrapeWebsiteAndPopulateScoreAsync(SquadronObj sqdobj)
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(sqdobj.url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                string scorePath = "//*[@id='bodyRoot']/div[4]/div[2]/div[3]/div/section/div[2]/div[3]/div[2]/div[1]/div[2]";

                HtmlNode score = doc.DocumentNode.SelectSingleNode(scorePath);
                string value = score.InnerText;

                sqdobj.Score = Int32.Parse(value);

                string digitsOnly = Regex.Replace(value, @"\D", "");
                char[] charsToTrim = { '"' };

                int number = Int32.Parse(digitsOnly);

                return sqdobj;

            }



        }

        //Returns a String
        public static async Task<String> TestScrape()
        {
            string url = "https://warthunder.com/en/community/getclansleaderboard/dif/_hist/page/1/sort/dr_era5";
            string rawData = await DownloadPageAsync(url);
            string[] chunks = SplitDataIntoChunks(rawData);
            
            int numSquadronsToScrape = Math.Min(21, chunks.Length); // Limit to first XX squadrons
            StringBuilder sb = new StringBuilder();
            sb.Append("#   Name   Wins     Loss  Played\n\n");

            for (int i = 1; i < numSquadronsToScrape; i++)
            {
                string chunk = chunks[i];
                
                string squadronName = ExtractFieldValue(chunk, "tag");
                if (squadronName.Length > 2)
                {
                    // Trim the first and last characters
                    squadronName = squadronName.Substring(1, squadronName.Length - 2);
                }
                squadronName = squadronName.PadRight(5, ' ');
                string battlesPlayed = ExtractFieldValue(chunk, "battles_hist").PadRight(5, ' ');
                string wins = ExtractFieldValue(chunk, "wins_hist").PadLeft(5, ' ');
                string score = ExtractFieldValue(chunk, "dr_era5_hist").PadRight(6, ' ');
                string pos = i.ToString().PadRight(3, ' ');
                int losses = int.Parse(battlesPlayed) - int.Parse(wins);
                string lossesPad = losses.ToString().PadLeft(6, ' ');

                //Console.WriteLine($"{i} {squadronName}: Battles Played - {battlesPlayed}, Wins - {wins}, Score: {score}");
            
                sb.Append($"{pos} {squadronName}: {wins} & {lossesPad} ({battlesPlayed}), Score: {score}\n");
            }
            return sb.ToString();
            
        }

        //Returns a list of SquadronObjs as an array
        public static async Task<SquadronObj[]> TestScrape2()
        {
            string url = "https://warthunder.com/en/community/getclansleaderboard/dif/_hist/page/1/sort/dr_era5";
            string rawData = await DownloadPageAsync(url);
            string[] chunks = SplitDataIntoChunks(rawData);

            int numSquadronsToScrape = Math.Min(21, chunks.Length); // Limit to first XX squadrons
            
            List<SquadronObj> squadrons = new List<SquadronObj>();
            

            for (int i = 1; i < numSquadronsToScrape; i++)
            {
                string chunk = chunks[i];

                string squadronName = ExtractFieldValue(chunk, "tag");
                if (squadronName.Length > 2)
                {
                    // Trim the first and last characters
                    squadronName = squadronName.Substring(1, squadronName.Length - 2);
                }

                string battlesPlayedStr = ExtractFieldValue(chunk, "battles_hist");
                string winsStr = ExtractFieldValue(chunk, "wins_hist");
                string scoreStr = ExtractFieldValue(chunk, "dr_era5_hist");

                int battlesPlayed = int.Parse(battlesPlayedStr);
                int wins = int.Parse(winsStr);
                int losses = battlesPlayed - wins;
                int score = int.Parse(scoreStr);

                int pos = i;

                SquadronObj squadron = new SquadronObj
                {
                    SquadronName = squadronName,
                    Wins = wins,
                    Losses = losses,
                    BattlesPlayed = battlesPlayed,
                    Score = score,
                    Pos = pos
                };

                //Console.WriteLine($"{i} {squadronName}: Battles Played - {battlesPlayed}, Wins - {wins}, Score: {score}");
                squadrons.Add(squadron);
                //sb.Append($"{pos} {squadronName}: {wins} & {lossesPad} ({battlesPlayed}), Score: {score}\n");
            }
            //return sb.ToString();
            return squadrons.ToArray();

        }

        //private static async Task<string> DownloadPageAsync(string url)
        //{
        //    using (HttpClient client = new HttpClient())
        //    {
        //        return await client.GetStringAsync(url);
        //    }
        //}

        private static async Task<string> DownloadPageAsync(string url, int maxRetries = 3, int delayMilliseconds = 2000)
        {
            using (HttpClient client = new HttpClient())
            {
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        // Attempt to download the page
                        return await client.GetStringAsync(url);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"Attempt {attempt} failed with error: {ex.Message}");

                        // If it's the last attempt, rethrow the exception
                        if (attempt == maxRetries)
                        {
                            Console.WriteLine("Max retries reached. Unable to load the page.");
                            throw;
                        }

                        // Wait for a delay before retrying
                        Console.WriteLine($"Retrying in {delayMilliseconds} ms...");
                        await Task.Delay(delayMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        // Handle other unexpected exceptions
                        Console.WriteLine($"Unexpected error on attempt {attempt}: {ex.Message}");
                        if (attempt == maxRetries)
                        {
                            throw;
                        }
                        await Task.Delay(delayMilliseconds);
                    }
                }
            }

            // Fallback return (should never reach here unless retries fail completely)
            return string.Empty;
        }


        private static string[] SplitDataIntoChunks(string rawData)
        {
            // Split the rawData into chunks based on the start of each squadron's data
            string[] separators = { "{\"pos\"" };
            return rawData.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string ExtractFieldValue(string chunk, string fieldName)
        {
            string pattern = $"\"{fieldName}\":(.*?),";
            Match match = Regex.Match(chunk, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim('\"', '{', '}');
            }
            return "N/A";
        }

        public static async Task<SquadronObj> ScrapeCheck(string message)
        {
            string sqdToGet;
            if (message.ToLower() == "!check bofss")
            {
                sqdToGet = "bofss";
            }
            if (message.ToLower() == "!check bufss")
            {
                sqdToGet = "bufss";
            }
            else
            {
                sqdToGet = message.Substring(7).Trim();
            }



            for (int page = 1; page <= 6; page++) // Iterate over 6 pages
            {
                string url = $"https://warthunder.com/en/community/getclansleaderboard/dif/_hist/page/{page}/sort/dr_era5";
                string rawData = await DownloadPageAsync(url);
                string[] chunks = SplitDataIntoChunks(rawData);

                foreach (string chunk in chunks)
                {
                    string squadronName = ExtractFieldValue(chunk, "tag");
                    if (squadronName.Length > 2)
                    {
                        // Trim the first and last characters
                        squadronName = squadronName.Substring(1, squadronName.Length - 2);
                    }

                    if (sqdToGet == "bofss")
                    {
                        sqdToGet = "BofSs";
                    }
                    if (sqdToGet == "bufss")
                    {
                        sqdToGet = "BufSs";
                    }


                    if (squadronName == sqdToGet)
                    {
                        string battlesPlayedStr = ExtractFieldValue(chunk, "battles_hist");
                        string winsStr = ExtractFieldValue(chunk, "wins_hist");
                        string scoreStr = ExtractFieldValue(chunk, "dr_era5_hist");

                        int battlesPlayed = int.Parse(battlesPlayedStr);
                        int wins = int.Parse(winsStr);
                        int losses = battlesPlayed - wins;
                        int score = int.Parse(scoreStr);

                        int pos = int.Parse(ExtractPlaceValue(chunk));

                        SquadronObj squadron = new SquadronObj
                        {
                            SquadronName = squadronName,
                            Wins = wins,
                            Losses = losses,
                            BattlesPlayed = battlesPlayed,
                            Score = score,
                            Pos = pos
                        };

                        return squadron; // Return the squadron info
                    }
                }
            }

            return null; // If BufSs is not found, return null
        }

        private static string ExtractPlaceValue(string chunk)
        {
            // Find the index of the colon and comma in the chunk
            int colonIndex = chunk.IndexOf(':');
            int commaIndex = chunk.IndexOf(',');

            // If both the colon and comma are found
            if (colonIndex != -1 && commaIndex != -1)
            {
                // Extract the substring between the colon and comma
                string placeSubstring = chunk.Substring(colonIndex + 1, commaIndex - colonIndex - 1).Trim();

                // Return the extracted value
                return placeSubstring;
            }

            // Return null if the colon or comma is not found
            return null;
        }


    }










}
 
