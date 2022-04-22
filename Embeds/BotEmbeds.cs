using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using GameMasterBot.Models.Entities;

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
        
        public static Embed Tutorial() =>
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
                        Value = "Once you have created a campaign, you can use the `/session schedule` command to plan a session. This can be used to create a standalone session or a recurring schedule, either weekly, fortnightly, or monthly. Players are reminded of a session 30 minutes before, and once more when the session begins.",
                        IsInline = false
                    },
                    new()
                    {
                        Name = "Further Help",
                        Value = "To get further help with this bot, annoy Killian. That's all you can do really.",
                        IsInline = false
                    }
                }
            }.Build();

        public static Embed SessionInfo(string title, Session session)
        {
            var tzInfoGm = TimeZoneInfo.FindSystemTimeZoneById(session.Campaign.GameMaster.User.TimeZoneId);
            var localisedTimestampGm = TimeZoneInfo.ConvertTimeFromUtc(session.Timestamp, tzInfoGm);
            var participants = $"<@{session.Campaign.GameMaster.User.DiscordId}> *(Game Master)*\n";
            var localisedDateTimes = $"{localisedTimestampGm:g} *({tzInfoGm.Id})*\n";
            foreach (var player in session.Campaign.Players)
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(player.User.TimeZoneId);
                var localisedTimestamp = TimeZoneInfo.ConvertTimeFromUtc(session.Timestamp, tzInfo);
                participants += $"<@{player.User.DiscordId}>\n";
                localisedDateTimes += $"{localisedTimestamp:g} *({tzInfo.Id})*\n";
            }
            return new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(title).WithIconUrl(IconUrl),
                Description = "***Note:** All session times are shown in the participants' respective timezones. If someone has an incorrect timezone, they can use `/timezone set` to set the correct one.*",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "Participant",
                        Value = participants,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Localised Date/Time",
                        Value = localisedDateTimes,
                        IsInline = true
                    }
                }
            }.Build();
        }

        public static Embed SessionList(string title, List<Session> sessions)
        {
            string dates = "", times = "";
            foreach (var session in sessions)
            {
                dates += session.Timestamp.ToShortDateString() + "\n";
                times += session.Timestamp.ToString("HH:mm") + "\n";
            }
            return new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(title).WithIconUrl(IconUrl),
                Description = $"***Note:** All session times are shown in your timezone (). If your timezone is incorrect, you can use `/timezone set` to set the correct one.*",
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
    }
}
