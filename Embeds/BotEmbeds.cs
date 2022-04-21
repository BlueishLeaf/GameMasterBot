using System.Collections.Generic;
using System.Linq;
using Discord;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Embeds
{
    public static class BotEmbeds
    {
        private const string IconUrl = "https://cdn.discordapp.com/avatars/597026097166680065/5fd03a7d9efa4f8cca8395e5555f4879.png?size=32";
        public static Embed CampaignInfo(Campaign campaign) =>
            new EmbedBuilder
            {
                Author = campaign.Url != null ? new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl) : new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(IconUrl),
                Color = Color.Purple,
                Footer = new EmbedFooterBuilder().WithText($"Campaign created on {campaign.CreatedAt:d}"),
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "System",
                        Value = campaign.System,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Game Master",
                        Value = $"<@{campaign.GameMaster.User.DiscordId}>",
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Players",
                        Value = campaign.Players.Count > 0 ? string.Join(", ", campaign.Players.Select(p => $"<@{p.User.DiscordId}>")) : "No players.",
                        IsInline = false
                    }
                }
            }.Build();
        
        public static Embed Overview() =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("How to use Game Master Bot").WithIconUrl(IconUrl),
                Description = "**Note:** Before continuing with this bot, ensure that a role called 'Whitelisted' exists on the server. A user requires this role in order to create a campaign.",
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "Creating a Campaign",
                        Value = "Creating a campaign is as simple as using the `/campaign create` command. This will not only create the campaign in the bot's system, but also create the relevant channels and roles.",
                        IsInline = false
                    },
                    new()
                    {
                        Name = "Scheduling a Session",
                        Value = "Once you have created a campaign, you can use the `/session schedule` command to plan a session. This can be used to create a standalone session or a recurring schedule, either Weekly, Fortnightly, or Monthly. Players are reminded of a session 30 minutes before, and once more when the session begins.",
                        IsInline = false
                    },
                    new()
                    {
                        Name = "Further Help",
                        Value = "To get further help with this bot, annoy Killian.",
                        IsInline = false
                    }
                }
            }.Build();

        public static Embed SessionInfo(string title, Session session) =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(title).WithIconUrl(IconUrl),
                Description = "*Note: All session times are given in Universal Time(UTC), use `/timezone convert 'time'` to convert to local time.*",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "Date",
                        Value = session.Timestamp.ToShortDateString(),
                        IsInline = true
                    },
                    new()
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
            return new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(title).WithIconUrl(IconUrl),
                Description = "*Note: All session times are given in Universal Time(UTC), use `/timezone convert 'time'` to convert to local time.*",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "Date",
                        Value = dates,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Time",
                        Value = times,
                        IsInline = true
                    }
                }
            }.Build();
        }
        
        // TODO: Tidy up boi
        public static Embed CampaignSessionSummary(Campaign campaign)
        {
            var sessions = campaign.Sessions.Where(s => s.State != SessionState.Archived).ToList();
            if (!sessions.Any())
                return new EmbedBuilder
                {
                    Author = campaign.Url != null ? new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl) : new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(IconUrl),
                    Color = Color.Purple,
                    Footer = new EmbedFooterBuilder().WithText($"Campaign created on {campaign.CreatedAt:d}"),
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new()
                        {
                            Name = "System",
                            Value = campaign.System,
                            IsInline = true
                        },
                        new()
                        {
                            Name = "Game Master",
                            Value = $"<@{campaign.GameMaster.User.DiscordId}>",
                            IsInline = true
                        },
                        new()
                        {
                            Name = "Players",
                            Value = campaign.Players.Count > 0 ? string.Join(", ", campaign.Players.Select(p => $"<@{p.User.DiscordId}>")): "No players.",
                            IsInline = true
                        },
                        new()
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
            return new EmbedBuilder
            {
                Author = campaign.Url != null ? new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl) : new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(IconUrl),
                Color = Color.Purple,
                Footer = new EmbedFooterBuilder().WithText($"Campaign created on {campaign.CreatedAt:d}"),
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "System",
                        Value = campaign.System,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Game Master",
                        Value = $"<@{campaign.GameMaster.User.DiscordId}>",
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Players",
                        Value = string.Join(", ", campaign.Players.Select(p => $"<@{p.User.DiscordId}>")),
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Upcoming Sessions",
                        Value = "*Note: All session times are given in Universal Time(UTC), use `/timezone convert 'time'` to convert to local time.*",
                        IsInline = false
                    },
                    new()
                    {
                        Name = "Date",
                        Value = dates,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Time",
                        Value = times,
                        IsInline = true
                    }
                }
            }.Build();
        }
    }
}
