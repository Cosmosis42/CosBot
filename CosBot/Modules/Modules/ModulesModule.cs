using Discord;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosBot.Modules.Modules
{
    //TODO: Save what modules have been enabled on each server
    internal class ModulesModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private ModuleService _service;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;
            _service = manager.Client.GetService<ModuleService>();

            manager.CreateCommands("modules", group =>
            {
                group.MinPermissions((int)PermissionLevel.ServerAdmin);

                group.CreateCommand("list")
                    .Description("Gives a list of all available modules.")
                    .Do(async e =>
                    {
                        string text = "Available Modules: " + string.Join(", ", _service.Modules.Select(x => x.Id));

                        if (e.User.PrivateChannel == null)
                            await e.User.CreatePMChannel();

                        await e.User.PrivateChannel.SendMessage(text);
                        //await _client.Reply(e.User, e.User.PrivateChannel, text);
                    });

                group.CreateCommand("enable")
                    .Description("Enables a module for this server.")
                    .Parameter("module")
                    .PublicOnly()
                    .Do(e =>
                    {
                        var module = GetModule(e.Args[0]);
                        if (module == null)
                        {
                            _client.ReplyError(e, "Unknown module");
                            return;
                        }
                        if (module.FilterType == ModuleFilter.None || module.FilterType == ModuleFilter.AlwaysAllowPrivate)
                        {
                            _client.ReplyError(e, "This module is global and cannot be enabled/disabled.");
                            return;
                        }
                        if (!module.FilterType.HasFlag(ModuleFilter.ServerWhitelist))
                        {
                            _client.ReplyError(e, "This module doesn't support being enabled for servers.");
                            return;
                        }
                        var server = e.Server;
                        if (!module.EnableServer(server))
                        {
                            _client.ReplyError(e, $"Module {module.Id} was already enabled for server {server.Name}.");
                            return;
                        }

                        // Save which module is enabled
                        for (int i = 0; i < ModuleSettings.Module.Length; i++)
                        {
                            if (e.Args[0].ToLower() == ModuleSettings.Module[i].name.ToLower())
                            {
                                ulong[] newServers = new ulong[ModuleSettings.Module[i].servers.Length + 1];
                                for (int j = 0; j < ModuleSettings.Module[i].servers.Length; j++)
                                {
                                    newServers[j] = ModuleSettings.Module[i].servers[j];
                                }
                                newServers[ModuleSettings.Module[i].servers.Length] = e.Server.Id;
                                ModuleSettings.Module[i].servers = newServers;
                            }
                        }

                        ModuleSettings.Save();

                        _client.Reply(e, $"Module {module.Id} was enabled for server {server.Name}.");
                    });

                group.CreateCommand("disable")
                    .Description("Disables a module for this server.")
                    .Parameter("module")
                    .PublicOnly()
                    .Do(e =>
                    {
                        var module = GetModule(e.Args[0]);
                        if (module == null)
                        {
                            _client.ReplyError(e, "Unknown module");
                            return;
                        }
                        if (module.FilterType == ModuleFilter.None || module.FilterType == ModuleFilter.AlwaysAllowPrivate)
                        {
                            _client.ReplyError(e, "This module is global and cannot be enabled/disabled.");
                            return;
                        }
                        if (!module.FilterType.HasFlag(ModuleFilter.ServerWhitelist))
                        {
                            _client.ReplyError(e, "This module doesn't support being enabled for servers.");
                            return;
                        }
                        var server = e.Server;
                        if (!module.DisableServer(server))
                        {
                            _client.ReplyError(e, $"Module {module.Id} was not enabled for server {server.Name}.");
                            return;
                        }

                        // Save which module is disabled
                        for (int i = 0; i < ModuleSettings.Module.Length; i++)
                        {
                            if (e.Args[0].ToLower() == ModuleSettings.Module[i].name.ToLower())
                            {
                                ulong[] newServers = new ulong[ModuleSettings.Module[i].servers.Length - 1];

                                for (int j = 0; j < ModuleSettings.Module[i].servers.Length; i++)
                                {
                                    if (e.Server != _client.GetServer(ModuleSettings.Module[i].servers[j]))
                                        newServers[i] = ModuleSettings.Module[i].servers[j];
                                }

                                newServers[ModuleSettings.Module[i].servers.Length] = e.Server.Id;
                                ModuleSettings.Module[i].servers = newServers;
                            }
                        }
                        ModuleSettings.Save();

                        _client.Reply(e, $"Module {module.Id} was disabled for server {server.Name}.");
                    });

                group.CreateCommand("reload")
                .Description("Reload all modules that were enabled on your server.")
                .PublicOnly()
                .Do(e =>
                {
                    for (int i = 0; i < ModuleSettings.Module.Length; i++)
                    {
                        var module = GetModule(ModuleSettings.Module[i].name);

                        for (int j = 0; j < ModuleSettings.Module[i].servers.Length; j++)
                        {
                            if (module == null)
                            {
                                _client.ReplyError(e, "Unknown module");
                                return;
                            }
                            if (module.FilterType == ModuleFilter.None || module.FilterType == ModuleFilter.AlwaysAllowPrivate)
                            {
                                _client.ReplyError(e, "This module is global and cannot be enabled/disabled.");
                                return;
                            }
                            if (!module.FilterType.HasFlag(ModuleFilter.ServerWhitelist))
                            {
                                _client.ReplyError(e, "This module doesn't support being enabled for servers.");
                                return;
                            }
                            var server = ModuleSettings.Module[i].servers[j];
                            if (!module.EnableServer(_client.GetServer(server)))
                            {
                                _client.ReplyError(e, $"Module {module.Id} was already enabled for server {_client.GetServer(server).Name}.");
                                return;
                            }
                        }
                    }

                    _client.Reply(e, "Beep Boop. All modules have been re-enabled for all servers.");
                });
            });
        }

        private ModuleManager GetModule(string id)
        {
            id = id.ToLowerInvariant();
            return _service.Modules.Where(x => x.Id == id).FirstOrDefault();
        }

    }
}
