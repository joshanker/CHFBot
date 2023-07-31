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
                for (int i = 8; i < 6 * number + 6; i = i + 6)
                {

                    Player newp = new Player();

                    //string namexpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + i + "]/a";

                    //string namexpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + i + "]/div";

                    //HtmlNode node = doc.DocumentNode.SelectSingleNode(namexpath);
                    //String plrname = node.InnerText.Trim('\n').Trim();
                    //newp = objname.setName(newp, plrname);

                    string namexpath = "//*[@id=\'bodyRoot\']/div[4]/div[2]/div[3]/div/section/div[3]/div/div[" + i + "]/a";
                    HtmlNode node = doc.DocumentNode.SelectSingleNode(namexpath);

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
                    String DoE = node.InnerText.Trim();
                    newp = objname.setDoE(newp, DoE);

                    objname.AddPlayerTolist(newp);

                }

                return objname;


                //objname.PrintSquadronInfo();

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

                //Console.ReadLine();


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


        }
    } 
