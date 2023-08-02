using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;



namespace SquadronObjects
{
        public class Player
    {
        //public string Name { get; set; }
        public int Number { get; set; }
        public string PlayerName { get; set; }
        public int PersonalClanRating { get; set; }
        public string Activity { get; set; }
        public string Rank { get; set; }
        public DateTime DateOfEntry { get; set; }

        public Player()
        {
        }

        public Player(string playerName, int number, int personalClanRating, string activity, string rank, DateTime dateOfEntry)
        {
            PlayerName = playerName;
            Number = number;
            PersonalClanRating = personalClanRating;
            Activity = activity;
            Rank = rank;
            DateOfEntry = dateOfEntry;
        }




    }


    public class SquadronObj
    {

        public string SquadronName { get; set; }
        public List<Player> Players { get; set; }
        public string allsqd { get; set; }
        public int totalRating { get; set; }
        public Boolean isValidSquadron { get; set; }
        public string url { get; set; }
        public int Score { get; set; }   


        public SquadronObj()
        {
                        
        }

        public SquadronObj(string squadronName, string url)
        {
            SquadronName = squadronName;
            Players = new List<Player>();
            url = url;
        }
            
            public Player setName(Player p, string n)
        {
            p.PlayerName = n;
            return p;
        }
        
        public Player setNumber(Player p, int num)
        {
            //p.Number = num.ToInt32();
            //p.Number = Int16.Parse(num);
            p.Number = num;
            return p;
        }

        public Player setRating(Player p, int num)
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
        public Player setDoE(Player p, DateTime num)
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

        
    }




}
