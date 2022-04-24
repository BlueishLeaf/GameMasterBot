using System.Collections.Generic;
using Discord;
using GameMasterBot.Constants;

namespace GameMasterBot.Utils;

public class InfoEmbedBuilder
{
    public static Embed BuildTutorialEmbed() =>
        new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder().WithName("How to use Game Master Bot").WithIconUrl(EmbedConstants.IconUrl),
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
        }.Build();}