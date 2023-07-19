using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SquadronObjects
{
        public class Player
    {
        //public string Name { get; set; }
        public String Number { get; set; }
        public string PlayerName { get; set; }
        public string PersonalClanRating { get; set; }
        public string Activity { get; set; }
        public string Rank { get; set; }
        public String DateOfEntry { get; set; }
    }


    public class SquadronObj
    {

        public string SquadronName { get; set; }
        public List<Player> Players { get; set; }
        public string sqdurl { get; set; }
        public string allsqd { get; set; }


        public SquadronObj(string squadronName, string url)
        {
            SquadronName = squadronName;
            Players = new List<Player>();
            sqdurl = url;
        }
            
            public Player setName(Player p, string n)
        {
            p.PlayerName = n;
            return p;
        }
        
        public Player setNumber(Player p, string num)
        {
            //p.Number = num.ToInt32();
            //p.Number = Int16.Parse(num);
            p.Number = num;
            return p;
        }

        public Player setRating(Player p, string num)
        {
            //p.Number = num.ToInt32();
            //p.Number = Int16.Parse(num);
            p.PersonalClanRating = num;
            return p;
        }
        public Player setActivity(Player p, string num)
        {
            //p.Number = num.ToInt32();
            //p.Number = Int16.Parse(num);
            p.Activity = num;
            return p;
        }
        public Player setRank(Player p, string num)
        {
            //p.Number = num.ToInt32();
            //p.Number = Int16.Parse(num);
            p.Rank = num;
            return p;
        }
        public Player setDoE(Player p, String num)
        {
            //p.Number = num.ToInt32();
            //p.Number = Int16.Parse(num);
            p.DateOfEntry = num;
            return p;
        }


        public void AddPlayerTolist(Player player)
            {
                Players.Add(player);
            }

            public void RemovePlayerFromlist(Player player)
            {
                Players.Remove(player);
            }



        //public void PrintSquadronInfo()
        //    {
        //        Console.WriteLine("Squadron: " + SquadronName);
        //        Console.WriteLine("Player Count: " + Players.Count);

        //        foreach (Player player in Players)
        //        {
        //            Console.WriteLine("Number: " + player.Number);
        //            Console.WriteLine("Player Name: " + player.PlayerName.TrimStart());
        //            Console.WriteLine("Personal Clan Rating: " + player.PersonalClanRating);
        //            Console.WriteLine("Activity: " + player.Activity);
        //            Console.WriteLine("Role: " + player.Rank);
        //            Console.WriteLine("Date of Entry: " + player.DateOfEntry);
        //            Console.WriteLine();
        //        }
        //    }
        


    }
}
