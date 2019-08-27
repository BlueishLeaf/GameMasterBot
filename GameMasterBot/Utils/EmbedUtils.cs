﻿using System.Collections.Generic;
using System.Linq;
using Common.Interfaces.Entities.Core;
using Discord;
using Discord.Commands;

namespace GameMasterBot.Utils
{
    public class EmbedUtils
    {
        private const string IconUrl = "https://cdn.discordapp.com/avatars/597026097166680065/5fd03a7d9efa4f8cca8395e5555f4879.png?size=32";
        public static Embed CampaignEmbed(ICampaign campaign) =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(IconUrl),
                Description = "For a list of all campaigns on this server, use the `!campaign server` or `!campaign *` matches.",
                Color = Color.Purple,
                Footer = new EmbedFooterBuilder().WithText($"Created By: {campaign.CreatedBy}"),
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
                        Value = campaign.GameMaster,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Players",
                        Value = string.Join(", ", campaign.Players),
                        IsInline = false
                    }
                }
            }.Build();

        public static Embed CampaignsEmbed(IEnumerable<ICampaign> campaigns) =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("All Campaigns on this Server").WithIconUrl(IconUrl),
                Description = "For more info on a specific campaign, use the `!campaign info 'campaign'` command.",
                Color = Color.Blue,
                Fields = BuildFieldsForCampaigns(campaigns)
            }.Build();

        public static Embed CommandsEmbed(IEnumerable<CommandMatch> commands) =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("Commands Matching your Search").WithIconUrl(IconUrl),
                Description = "For a list of all matches, use the `!help` command.",
                Color = Color.Orange,
                Fields =  BuildFieldsForCommands(commands)
            }.Build();

        public static Embed ModulesEmbed(IEnumerable<ModuleInfo> modules) =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("All Commands").WithIconUrl(IconUrl),
                Description = "For help with a specific command, use the `!help 'command'` command.",
                Color = Color.Red,
                Fields = BuildFieldsForModules(modules)
            }.Build();

        private static List<EmbedFieldBuilder> BuildFieldsForCommands(IEnumerable<CommandMatch> matches) => 
            matches.Select(match => new EmbedFieldBuilder
            {
                Name = match.Command.Module.Group != null ? $"**!{match.Command.Module.Group}** " + string.Join(", ", match.Command.Aliases) : "!" + string.Join(", !", match.Command.Aliases),
                Value = $"***Summary:*** {match.Command.Summary}\n***Parameters:*** {(match.Command.Parameters.Any() ? string.Join(", ", match.Command.Parameters.Select(parameter => $"*{parameter.Name}*")) : "None")}", IsInline = false
            }).ToList();

        private static List<EmbedFieldBuilder> BuildFieldsForModules(IEnumerable<ModuleInfo> modules) =>
            (from module in modules let description = module.Commands.Aggregate<CommandInfo, string>(null, (current, command) => current + $"`{command.Name}` - {command.Summary}\n")
            select new EmbedFieldBuilder {Name = module.Group != null ? $"{module.Name} Commands - `!{module.Group} 'command'`": $"{module.Name} Commands", Value = description, IsInline = false}).ToList();

        private static List<EmbedFieldBuilder> BuildFieldsForCampaigns(IEnumerable<ICampaign> campaigns)
        {
            var builders = new List<EmbedFieldBuilder>();
            foreach (var campaign in campaigns)
            {
                builders.Add(new EmbedFieldBuilder
                {
                    Name = "Campaign",
                    Value = campaign.Name,
                    IsInline = true
                });
                builders.Add(new EmbedFieldBuilder
                {
                    Name = "System",
                    Value = campaign.System,
                    IsInline = true
                });
                builders.Add(new EmbedFieldBuilder
                {
                    Name = "Game Master",
                    Value = campaign.GameMaster,
                    IsInline = true
                });
            }
            return builders;
        }
    }
}
