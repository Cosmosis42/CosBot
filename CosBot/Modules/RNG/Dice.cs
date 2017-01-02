using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosBot.Modules.RNG
{
    class Dice
    {
        /*
         * Rolls X d Y depending on the message entered by the user.
         *      Message may be in the form of XDY, XD, or DY
         *      If number of dice is not mentioned, roll 1.
         *      If number of sides is not mentioned, assume 6.
         *      Message MUST contain the letter D for parsing reasons.
         */
        public static async Task RollDice(CommandEventArgs e)
        {
            string diceToRoll = e.Message.Text.ToLower();
            int dLocation = 0;
            int numDice = 0;
            int numSides = 0;
            string firstNumText = "1";
            string secNumText = "6";
            string results = "";

            dLocation = diceToRoll.LastIndexOf('d');

            if (dLocation != -1)
            {
                // Lots of error handling when selecting substrings
                try
                {
                    firstNumText = diceToRoll.Substring(5, dLocation - 5);
                    Console.WriteLine(dLocation + "   " + firstNumText);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Assume only one die if error
                    firstNumText = "1";
                }

                // Lots of error handling when selecting substrings
                try
                {
                    secNumText = diceToRoll.Substring(dLocation + 1, diceToRoll.Length - dLocation - 1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Assume a regular d6 if error
                    secNumText = "6";
                }

                // Lots of error checking when converting to int
                if (Int32.TryParse(firstNumText, out numDice))
                {
                    if (numDice <= 100000)
                    {
                        if (Int32.TryParse(secNumText, out numSides))
                        {
                            // Tell the user which dice you're rolling
                            await e.Channel.SendMessage("Beep Boop. *Rolling " + numDice + "d" + numSides + "*");

                            // Print the results given by the dice roll function
                            results = DiceRoll(numDice, numSides);
                            await e.Channel.SendMessage(results);
                        }
                        else
                        {
                            // Assume 6 sides on error
                            numSides = 6;

                            // Tell the user which dice you're rolling
                            await e.Channel.SendMessage("Beep Boop. *Rolling " + numDice + "d" + numSides + "*");

                            // Print the results given by the dice roll function
                            results = DiceRoll(numDice, numSides);
                            await e.Channel.SendMessage(results);
                        }
                    }
                    else
                        // This is to prevent timeouts. No more than 100 000 dice can be rolled at once.
                        // Not sure why you would do that anyway
                        await e.Channel.SendMessage("Beep Boop. I don't want to roll that many.");
                }
                else
                {
                    if (Int32.TryParse(secNumText, out numSides))
                    {
                        //Assume 1 die on error
                        numDice = 1;

                        // Tell the user which dice you're rolling
                        await e.Channel.SendMessage("Beep Boop. *Rolling " + numDice + "d" + numSides + "*");

                        // Print the results given by the diceroll function
                        results = DiceRoll(numDice, numSides);
                        await e.Channel.SendMessage(results);
                    }
                    else
                    {
                        // Assume 6 sides on error
                        numDice = 1;
                        numSides = 6;

                        // Tell the user which dice you're rolling
                        await e.Channel.SendMessage("Beep Boop. *Rolling " + numDice + "d" + numSides + "*");

                        // Print the results given by the diceroll function
                        results = DiceRoll(numDice, numSides);
                        await e.Channel.SendMessage(results);
                    }
                }
            }
            else
            {
                // Parsing will fail if no D is found.
                await e.Channel.SendMessage("Beep Boop. I need the D.");
            }
        }

        /*
         * This is where the diceroll messages are actually generated.
         */
        private static string DiceRoll(int numDice, int numSides)
        {
            string ret = "";
            int tot = 0;
            int cur = 0;
            Random rnd = new Random();

            // Roll all the dice, find the total, and make a list of results in the form of a string
            for (int i = 0; i < numDice; i++)
            {
                if (i != numDice - 1)
                {
                    cur = rnd.Next(1, numSides + 1);
                    tot += cur;
                    ret += cur + ", ";
                }
                // Make sure there are no extra ','s
                else
                {
                    cur = rnd.Next(1, numSides + 1);
                    tot += cur;
                    ret += cur;
                }
            }

            // Message is informative.
            // Also, concatenate the results and return as a string as this is the easiest way to pass the total and 
            //      the list of results
            ret = "For a total of " + tot + "," + "You rolled: " + ret;

            // If the message is too long, the bot may crash. We don't want that
            if (ret.Length > 1750)
                return ret.Substring(0, 1750) + "...";
            else
                return ret;
        }
    }
}
