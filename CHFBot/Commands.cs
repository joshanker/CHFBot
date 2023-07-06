using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BotCommands
{

    public class Commands
    {
      
        public string getQuote()
        {
            string quote = "quote in getQuote was not populated.";

            try
            {
                var lines = File.ReadAllLines("C:\\quotes.txt");
                var r = new Random();
                var randomLineNumber = r.Next(0, lines.Length - 1);
                var line = lines[randomLineNumber];
                return line;
             
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block of getQuote.");
            }
            
            return quote;
        }

           



    }
 }
