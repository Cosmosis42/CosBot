﻿using CosBot.Modules;
using CosBot.Modules.Admin;
using CosBot.Modules.Chatter;
using CosBot.Modules.Colors;
using CosBot.Modules.Convo;
using CosBot.Modules.Modules;
using CosBot.Modules.Public;
using CosBot.Modules.RNG;
using CosBot.Services;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * TODO:
 *      Spam detection - Timeout on commands
 *      Voting/polls/Raffle
 *      Google/Feeling Lucky
 *      Permissions
 *      Joke telling functions
 *      Role Modification
 *      LMGTFY
 *      Jap to Eng Dictionary
 *      Imgur search function - Filter NSFW
 *      Self delete on bad results
 *      More Beep Boops
 *      Emotes --> Memes, lenny... Maybe?
 *      D&D Related stuff (rule lookups, etc)
 *      reowrk dice roller?
 *      Quote function - Mods add quotes
 *      Profanity filter
 *      WORLD DOMINATION
 *      But first... Le nap. Then, FIRE ZE MISSILES
 */
namespace CosBot
{
   public class Program
    {
        public static void Main(string[] args) => new Program().Start(args);

        private const string AppName = "CosBot5000";
        private const string AppUrl = "TBD";

        private DiscordClient _client;

        private void Start(string[] args)
        {
#if !DNXCORE50
            Console.Title = $"{AppName} (Discord.Net v{DiscordConfig.LibVersion})";
#endif
            GlobalSettings.Load();
            ModuleSettings.Load();

            _client = new DiscordClient(x =>
            {
                x.AppName = AppName;
                x.AppUrl = AppUrl;
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = true;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Debug;
                x.LogHandler = OnLogMessage;
            })
            .UsingCommands(x =>
            {
                x.AllowMentionPrefix = false;
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Private;
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
            })
            .UsingModules()
            .UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                //x.EnableMultiserver = true; //This is currently not supported in Discord.net v0.9
                x.EnableEncryption = true;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            })
            .UsingPermissionLevels(PermissionResolver);

            _client.AddService<SettingsService>();
            _client.AddService<HttpService>();

            _client.AddModule<ModulesModule>("Modules", ModuleFilter.None);
            _client.AddModule<PublicModule>("Public", ModuleFilter.None);
            _client.AddModule<ColorsModule>("Colors", ModuleFilter.ServerWhitelist);
            _client.AddModule<RNGModule>("RNG", ModuleFilter.ServerWhitelist);
            _client.AddModule<AdminModule>("Admin", ModuleFilter.ServerWhitelist);
            _client.AddModule<ChatterModule>("Chatterbot", ModuleFilter.ServerWhitelist);
            _client.AddModule<ConvoModule>("Convo", ModuleFilter.ServerWhitelist);

#if PRIVATE
            PrivateModules.Install(_client);
#endif

            _client.MessageReceived += Bot_MessageReceived;

            

            //Convert this method to an async function and connect to the server
            //DiscordClient will automatically reconnect once we've established a connection, until then we loop on our end
            //Note: ExecuteAndWait is only needed for Console projects as Main can't be declared as async. UI/Web applications should *not* use this function.
             _client.ExecuteAndWait(async () =>
             {
                 while (true)
                 {
                     try
                     {
                         await _client.Connect(GlobalSettings.Discord.Token);
                         //_client.SetGame("Discord.Net");
                         //await _client.ClientAPI.Send(new Discord.API.Client.Rest.HealthRequest());
                         break;
                     }
                     catch (Exception ex)
                     {
                         _client.Log.Error($"Login Failed", ex);
                         await Task.Delay(_client.Config.FailedReconnectDelay);
                     }
                 }
             });
        }

        private void Bot_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;

            string message = e.Message.Text.ToLower();

            if (message.Contains("cosbot") || e.Message.Text.Contains("cosbot5000"))
            {

                if (message.Contains("cosbot5000"))
                    message.Replace("cosbot5000", "");
                else if (message.Contains("CosBot"))
                    message.Replace("cosbot", "");

                string reply = Talkback.ChatReply(message);
                e.Channel.SendMessage(reply);
            }
        }

        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.Message;
            if (msg == null)
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        msg = "Unkown Error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        msg = "Unkown Command.";
                        break;
                }
            }
            else if (msg != null)
            {
                _client.ReplyError(e, msg);
                _client.Log.Error("Command", msg);
            }
        }

        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            //Colour
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build Message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                // Get rid of the control characters
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        
        private int PermissionResolver(User user, Channel channel)
        {
            if (user.Id == GlobalSettings.Users.DevId)
                return (int)PermissionLevel.BotOwner;
            if (user.Server != null)
            {
                if (user == channel.Server.Owner)
                    return (int)PermissionLevel.ServerOwner;

                var serverPerms = user.ServerPermissions;
                if (serverPerms.ManageRoles)
                    return (int)PermissionLevel.ServerAdmin;
                if (serverPerms.ManageMessages && serverPerms.KickMembers && serverPerms.BanMembers)
                    return (int)PermissionLevel.ServerModerator;

                var channelPerms = user.GetPermissions(channel);
                if (channelPerms.ManagePermissions)
                    return (int)PermissionLevel.ChannelAdmin;
                if (channelPerms.ManageMessages)
                    return (int)PermissionLevel.ChannelModerator;
            }

            return (int)PermissionLevel.User;
        }
    }
}
