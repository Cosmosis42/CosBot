using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosBot.Modules.RNG
{
    internal class RNGModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            manager.CreateCommands("", group =>
            {
                group.MinPermissions((int)PermissionLevel.User);

                group.CreateCommand("roll")
                    .Description("Rolls any amount of dice with any amount of sides.")
                    .Parameter("XdY", ParameterType.Required)
                    //.Parameter("Modifier", ParameterType.Optional)
                    //.Parameter("Modify By", ParameterType.Optional)
                    .Do(async e =>
                    {
                        await Dice.RollDice(e);
                    });
            }); 
        }
    }
}
