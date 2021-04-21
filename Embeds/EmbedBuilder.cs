using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Embeds
{
    public static class EmbedBuilder
    {
        private const string IconUrl = "https://cdn.discordapp.com/avatars/597026097166680065/5fd03a7d9efa4f8cca8395e5555f4879.png?size=32";
        public static Embed CampaignInfo(Campaign campaign) =>
            new Discord.EmbedBuilder
            {
                Author = campaign.Url != null ? new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl) : new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(IconUrl),
                Color = Color.Purple,
                Footer = new EmbedFooterBuilder().WithText($"Created By: {campaign.GameMaster.User.Username}"),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "System",
                        Value = campaign.System,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Game Master",
                        Value = campaign.GameMaster.User.Username,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Players",
                        Value = campaign.Players.Count > 0 ? string.Join(", ", campaign.Players) : "No players.",
                        IsInline = false
                    }
                }
            }.Build();
        
        public static Embed Overview() =>
            new Discord.EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("How to use Game Master Bot").WithIconUrl(IconUrl),
                Description = "**Note:** Before continuing with this bot, ensure that a role called 'Whitelisted' exists on the server. A user requires this role in order to create a campaign.",
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Creating a Campaign",
                        Value = "Creating a campaign is as simple as using the `!campaign add` command. This will not only create the campaign in the bot's system, but also create the relevant channels and roles.",
                        IsInline = false
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Scheduling a Session",
                        Value = "Once you have created a campaign, you can use either the `!session add` or `!session schedule` commands to plan a session. The difference between the two commands is that `add` is used to create once-off AdHoc sessions whereas `schedule` is used to create a recurring session, either Weekly, Fortnightly, or Monthly. Players are reminded of a session 30 minutes before, and once more when the session begins.",
                        IsInline = false
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Further Help",
                        Value = "To get further help with this bot, you can use the `!help` command to get a list of all bot commands. For help with a particular command, use the `!help commandName` command.",
                        IsInline = false
                    }
                }
            }.Build();

        public static Embed SessionInfo(string title, Session session) =>
            new Discord.EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(title).WithIconUrl(IconUrl),
                Description = "*Note: All session times are given in Universal Time(UTC), use `!convert 'time'` to convert to local time.*",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Date",
                        Value = session.Timestamp.ToShortDateString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Time",
                        Value = session.Timestamp.ToString("HH:mm"),
                        IsInline = true
                    }
                }
            }.Build();

        public static Embed SessionList(string title, IEnumerable<Session> sessions)
        {
            sessions = sessions.ToList();
            string dates = "", times = "";
            foreach (var session in sessions)
            {
                dates += session.Timestamp.ToShortDateString() + "\n";
                times += session.Timestamp.ToString("HH:mm") + "\n";
            }
            return new Discord.EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(title).WithIconUrl(IconUrl),
                Description = "*Note: All session times are given in Universal Time(UTC), use `!convert 'time'` to convert to local time.*",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Date",
                        Value = dates,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Time",
                        Value = times,
                        IsInline = true
                    }
                }
            }.Build();
        }
        
        // TODO: Tidy up boi
        public static Embed CampaignSummary(Campaign campaign)
        {
            var sessions = campaign.Sessions.Where(s => s.State != SessionState.Archived).ToList();
            if (!sessions.Any())
                return new Discord.EmbedBuilder
                {
                    Author = campaign.Url != null ? new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl) : new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(IconUrl),
                    Description =
                        "For a list of all campaigns on this server, use the `!campaign server` command.",
                    Color = Color.Purple,
                    Footer = new EmbedFooterBuilder().WithText($"Created By: {campaign.GameMaster.User.Username}"),
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "System",
                            Value = campaign.System,
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Game Master",
                            Value = campaign.GameMaster.User.Username,
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Players",
                            Value = campaign.Players.Count > 0 ? string.Join(", ", campaign.Players): "No players.",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Upcoming Sessions",
                            Value = "No sessions currently scheduled.",
                            IsInline = false
                        }
                    }
                }.Build();
            string dates = "", times = "";
            foreach (var session in sessions)
            {
                dates += session.Timestamp.ToShortDateString() + "\n";
                times += session.Timestamp.ToString("HH:mm") + "\n";
            }
            return new Discord.EmbedBuilder
            {
                Author = campaign.Url != null ? new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl) : new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(IconUrl),
                Description =
                    "For a list of all campaigns on this server, use the `!campaign server` command.",
                Color = Color.Purple,
                Footer = new EmbedFooterBuilder().WithText($"Created By: {campaign.GameMaster.User.Username}"),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "System",
                        Value = campaign.System,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Game Master",
                        Value = campaign.GameMaster.User.Username,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Players",
                        Value = string.Join(", ", campaign.Players),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Upcoming Sessions",
                        Value = "*Note: All session times are given in Universal Time(UTC), use `!convert 'time'` to convert to local time.*",
                        IsInline = false
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Date",
                        Value = dates,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Time",
                        Value = times,
                        IsInline = true
                    }
                }
            }.Build();
        }

        public static Embed CommandList(IEnumerable<CommandMatch> commands) =>
            new Discord.EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("Commands Matching your Search").WithIconUrl(IconUrl),
                Description = "For a list of all commands, use the `!help` command.",
                Color = Color.Orange,
                Fields =  BuildFieldsForCommands(commands)
            }.Build();

        public static Embed ModuleList(IEnumerable<ModuleInfo> modules) =>
            new Discord.EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("All Commands").WithIconUrl(IconUrl),
                Description = "For help with a specific command, use the `!help 'command'` command.",
                Color = Color.Red,
                Fields = BuildFieldsForModules(modules)
            }.Build();

        private static List<EmbedFieldBuilder> BuildFieldsForCommands(IEnumerable<CommandMatch> matches) => 
            matches.Select(match => new EmbedFieldBuilder
            {
                Name = "`!" + string.Join("`, `!", match.Command.Aliases) + "`",
                Value = $"**Summary:** {match.Command.Summary}\n**Parameters:** {(match.Command.Parameters.Any() ? "\n" + string.Join("\n", match.Command.Parameters.Select(parameter => $"*{parameter.Name}:* {parameter.Summary}")) : "*None*")}", IsInline = false
            }).ToList();

        private static List<EmbedFieldBuilder> BuildFieldsForModules(IEnumerable<ModuleInfo> modules) =>
            (from module in modules let description = module.Commands.Aggregate("", (current, command) => current + $"`{command.Name}` - {command.Summary}\n")
            select new EmbedFieldBuilder {Name = module.Group != null ? $"{module.Name} Commands - `!{module.Group} 'command'`": $"{module.Name} Commands", Value = description, IsInline = false}).ToList();
    }
}
