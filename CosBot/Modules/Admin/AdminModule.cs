using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CosBot.Modules.Admin
{
    /// <summary> Provides easy access to manage users from chat. </summary>
    internal class AdminModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        private bool filterSpam = false;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", group =>
            {
                group.PublicOnly();

                group.CreateCommand("kick")
                    .Description("Kicks a user from this server.")
                    .Parameter("user")
                    .Parameter("discriminator", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.ServerModerator)
                    .Do(async e =>
                    {
                        var user = await _client.FindUser(e, e.Args[0], e.Args[1]);
                        if (user == null) return;

                        await user.Kick();
                        await _client.Reply(e, $"Kicked user {user.Name}.");
                    });

                group.CreateCommand("ban")
                    .Description("Bans a user from this server.")
                    .Parameter("user")
                    .Parameter("discriminator", ParameterType.Optional)
                    .MinPermissions((int)PermissionLevel.ServerModerator)
                    .Do(async e =>
                    {
                        var user = await _client.FindUser(e, e.Args[0], e.Args[1]);
                        if (user == null) return;

                        await user.Server.Ban(user);
                        await _client.Reply(e, $"Banned user {user.Name}.");
                    });

                group.CreateCommand("mute")
                    .Parameter("user")
                    .Parameter("discriminator", ParameterType.Optional)
                    .Description("Mutes a user.")
                    .MinPermissions((int)PermissionLevel.ServerModerator)
                    .Do(async e =>
                    {
                        var user = await _client.FindUser(e, e.Args[0], e.Args[1]);
                        if (user == null) return;

                        await user.Edit(isMuted: true);
                        await _client.Reply(e, $"Muted user {user.Name}.");
                    });

                group.CreateCommand("unmute")
                    .Parameter("user")
                    .Parameter("discriminator", ParameterType.Optional)
                    .Description("Unmute a user.")
                    .MinPermissions((int)PermissionLevel.ServerModerator)
                    .Do(async e =>
                    {
                        var user = await _client.FindUser(e, e.Args[0], e.Args[1]);
                        if (user == null) return;

                        await user.Edit(isMuted: false);
                        await _client.Reply(e, $"Unmuted user {user.Name}.");
                    });

                group.CreateCommand("deafen")
                    .Parameter("user")
                    .Parameter("discriminator", ParameterType.Optional)
                    .Description("Deafen a user.")
                    .MinPermissions((int)PermissionLevel.ServerModerator)
                    .Do(async e =>
                    {
                        var user = await _client.FindUser(e, e.Args[0], e.Args[1]);
                        if (user == null) return;

                        await user.Edit(isDeafened: true);
                        await _client.Reply(e, $"Deafened user {user.Name}.");
                    });

                group.CreateCommand("undeafen")
                    .Parameter("user")
                    .Parameter("discriminator", ParameterType.Optional)
                    .Description("Undeafen a user.")
                    .MinPermissions((int)PermissionLevel.ServerModerator)
                    .Do(async e =>
                    {
                        var user = await _client.FindUser(e, e.Args[0], e.Args[1]);
                        if (user == null) return;

                        await user.Edit(isDeafened: false);
                        await _client.Reply(e, $"Undeafened user {user.Name}.");
                    });

                group.CreateCommand("purge")
                    .Parameter("count")
                    .Parameter("user", ParameterType.Optional)
                    .Parameter("discriminator", ParameterType.Optional)
                    .Description("Delete a given amount of messages. Optionally include a user"
                    + " to only delete messages from them.")
                    .MinPermissions((int)PermissionLevel.ChannelModerator)
                    .Do(async e =>
                    {
                        int count = int.Parse(e.Args[0]);
                        string username = e.Args[1];
                        string discriminator = e.Args[2];
                        User[] users = null;

                        if (username != "")
                        {
                            users = await _client.FindUsers(e, username, discriminator);
                            if (users == null) return;
                        }

                        IEnumerable<Message> msgs;
                        var cachedMsgs = e.Channel.Messages;
                        if (cachedMsgs.Count() < count)
                            msgs = (await e.Channel.DownloadMessages(count));
                        else
                            msgs = e.Channel.Messages.OrderByDescending(x => x.Timestamp).Take(count);

                        if (username != "")
                            msgs = msgs.Where(x => users.Contains(x.User));

                        if (msgs.Any())
                        {
                            foreach (var msg in msgs)
                                await msg.Delete();
                            await _client.Reply(e, $"Cleaned up {msgs.Count()} messages.");
                        }
                        else
                            await _client.ReplyError(e, $"No messages found.");
                    });

                group.CreateCommand("rules")
                .Description("Prints the rules for the server.")
                .MinPermissions((int)PermissionLevel.ChannelModerator)
                .Do(async e =>
                {
                    for (int i = 0; i < ModuleSettings.Rules.server.Length; i++)
                    {
                        if (ModuleSettings.Rules.server[i] == e.Server.Id)
                            await e.Channel.SendMessage(ModuleSettings.Rules.ruleMessage[i]);
                    }
                });

                group.CreateCommand("setRules")
                .Parameter("Message", ParameterType.Unparsed)
                .Description("Set the rule message for the server.")
                .MinPermissions((int)PermissionLevel.ServerModerator)
                .Do(async e =>
                {
                    bool serverFlag = false;

                    // Save the rule message.
                    for (int i = 0; i < ModuleSettings.Rules.server.Length; i++)
                    {
                        if (ModuleSettings.Rules.server[i] == e.Server.Id)
                        {
                            ModuleSettings.Rules.ruleMessage[i] = e.Args[0];

                            serverFlag = true;
                        }
                    }

                    if (serverFlag == false)
                    {
                        ulong[] newServers = new ulong[ModuleSettings.Rules.server.Length + 1];
                        string[] newRules = new string[ModuleSettings.Rules.ruleMessage.Length + 1];

                        for (int i = 0; i < ModuleSettings.Rules.server.Length; i++)
                        {
                            newServers[i] = ModuleSettings.Rules.server[i];
                            newRules[i] = ModuleSettings.Rules.ruleMessage[i];
                        }
                        newServers[ModuleSettings.Rules.server.Length] = e.Server.Id;
                        newRules[ModuleSettings.Rules.ruleMessage.Length] = e.Args[0];
                        ModuleSettings.Rules.server = newServers;
                        ModuleSettings.Rules.ruleMessage = newRules;
                    }

                    ModuleSettings.Save();

                    await e.Channel.SendMessage("Beep Boop. Rule Message saved for " + e.Server.Name);
                });
            });
        }
    }
}