using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Discord;
using GameMasterBot.Constants;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Utils
{
    public static class SessionEmbedBuilder
    {
        public static Embed BuildSessionEmbed(Session session)
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
                Author = new EmbedAuthorBuilder().WithName($"{session.Campaign.Name} Session Details").WithIconUrl(EmbedConstants.IconUrl),
                Description = "***Note:** All session times are shown in the participants' respective timezones. If someone has an incorrect timezone, they can use `/timezone set` to set the correct one.*",
                Color = Color.Gold,
                Footer = new EmbedFooterBuilder().WithText(GetSessionFooterMessage(session.Frequency)),
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

        private static string GetSessionFooterMessage(ScheduleFrequency frequency)
        {
            return frequency switch
            {
                ScheduleFrequency.Standalone => "This is a standalone session.",
                ScheduleFrequency.Weekly => "This session re-occurs weekly.",
                ScheduleFrequency.Fortnightly => "This session re-occurs fortnightly.",
                ScheduleFrequency.Monthly => "This session re-occurs monthly.",
                _ => null
            };
        }

        public static Embed BuildSessionListEmbed(User viewingUser, List<Session> sessions)
        {
            var firstSession = sessions.First();
            string localisedDateTimes = "", frequencies = "";
            foreach (var session in sessions)
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(viewingUser.TimeZoneId);
                var localisedTimestamp = TimeZoneInfo.ConvertTimeFromUtc(session.Timestamp, tzInfo);
                localisedDateTimes += $"{localisedTimestamp:g}\n";
                frequencies += $"{session.Frequency.ToString()}\n";
            }
            return new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName($"Upcoming Scheduled Sessions for {firstSession.Campaign.Name}").WithIconUrl(EmbedConstants.IconUrl),
                Description = $"***Note:** All session times are shown in your timezone ({viewingUser.TimeZoneId}). If your timezone is incorrect, you can use `/timezone set` to set the correct one.*",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "Localised Date/Time",
                        Value = localisedDateTimes,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Frequency",
                        Value = frequencies,
                        IsInline = true
                    }
                }
            }.Build();
        }
    }
}
