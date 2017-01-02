using Discord.Modules;
using ChatterBotAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands.Permissions.Levels;
using Discord.Commands;

namespace CosBot.Modules.Chatter
{
    internal class ChatterModule : IModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            manager.CreateCommands("chatter", group =>
            {
                group.MinPermissions((int)PermissionLevel.User);

                group.CreateCommand("")
                    .Description("Talk to CosBot")
                    .Parameter("Text", ParameterType.Unparsed)
                    .MinPermissions((int)PermissionLevel.ChannelModerator)
                    .Do(async e =>
                    {
                        string reply = await Talkback.Chat(e.Args[0]);
                        await e.Channel.SendMessage(reply);
                    });
            });
            
        }
    }
}
