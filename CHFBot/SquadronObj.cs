using System;
using System.Collections.Generic;

namespace SquadronObjects
{
        public class Player
    {
        public int Number { get; set; }
        public string PlayerName { get; set; }
        public double PersonalClanRating { get; set; }
        public string Activity { get; set; }
        public string Role { get; set; }
        public DateTime DateOfEntry { get; set; }
    }


    public class SquadronObj
    {

        public string SquadronName { get; set; }
        public List<Player> Players { get; set; }


        public SquadronObj(string squadronName)
        {
            SquadronName = squadronName;
            Players = new List<Player>();
        }
            
            public void AddPlayer(Player player)
            {
                Players.Add(player);
            }

            public void PrintSquadronInfo()
            {
                Console.WriteLine("Squadron: " + SquadronName);
                Console.WriteLine("Player Count: " + Players.Count);

                foreach (Player player in Players)
                {
                    Console.WriteLine("Number: " + player.Number);
                    Console.WriteLine("Player Name: " + player.PlayerName);
                    Console.WriteLine("Personal Clan Rating: " + player.PersonalClanRating);
                    Console.WriteLine("Activity: " + player.Activity);
                    Console.WriteLine("Role: " + player.Role);
                    Console.WriteLine("Date of Entry: " + player.DateOfEntry);
                    Console.WriteLine();
                }
            }
        


    }
}
