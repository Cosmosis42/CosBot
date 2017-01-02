using Discord;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using ChatterBotAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace CosBot.Modules.Chatter
{
    class Talkback
    {
        static ChatterBotFactory factory = new ChatterBotFactory();

        static ChatterBot bot1 = factory.Create(ChatterBotType.CLEVERBOT);
        static ChatterBotSession bot1session = bot1.CreateSession();

        static ChatterBot bot2 = factory.Create(ChatterBotType.PANDORABOTS, "b0dafd24ee35a477");
        static ChatterBotSession bot2session = bot2.CreateSession();

        public static async Task<string> Chat(string message)
        {
            string reply = "";
            
            reply = bot1session.Think(message);
            //await e.Channel.SendMessage(e.Message.User.Mention + ": " + reply);
            return reply;
        }

        public static string ChatReply (string message)
        {
            string reply = "";

            reply = bot1session.Think(message);
            //await e.Channel.SendMessage(e.Message.User.Mention + ": " + reply);
            return reply;
        }
    }
}
