using System.Collections.Generic;
using System.Linq;
using Common.Interfaces.Entities.Core;
using Discord;

namespace GameMasterBot.Utils
{
    public class EmbedUtils
    {
        public static Embed CampaignEmbed(ICampaign campaign) =>
            new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl("https://s3.amazonaws.com/files.d20.io/marketplace/527279/nvsTfqKL38EeMpudZyiUeg/med.png?1526331193298"),
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
                Author = new EmbedAuthorBuilder().WithName($"{campaigns.First().ServerName} Campaigns").WithIconUrl("https://s3.amazonaws.com/files.d20.io/marketplace/527279/nvsTfqKL38EeMpudZyiUeg/med.png?1526331193298"),
                Color = Color.Blue,
                Fields = BuildFieldsForCampaigns(campaigns)
            }.Build();

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
