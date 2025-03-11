using System.Linq;
using Discord;
using GameMasterBot.Constants;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Utils;

public static class CampaignEmbedBuilder
{
    public static Embed BuildCampaignEmbed(Campaign campaign) =>
        new EmbedBuilder
        {
            Author = campaign.Url != null ? 
                new EmbedAuthorBuilder().WithName(campaign.Name).WithUrl(campaign.Url).WithIconUrl(EmbedConstants.IconUrl) : 
                new EmbedAuthorBuilder().WithName(campaign.Name).WithIconUrl(EmbedConstants.IconUrl),
            Color = Color.Purple,
            Footer = new EmbedFooterBuilder().WithText($"Campaign created on {campaign.CreatedAt:d}"),
            Fields =
            [
                new EmbedFieldBuilder
                {
                    Name = "System",
                    Value = campaign.System,
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Game Master",
                    Value = $"<@{campaign.GameMaster.User.DiscordId}>",
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Players",
                    Value = campaign.Players.Count > 0
                        ? string.Join(", ", campaign.Players.Select(p => $"<@{p.User.DiscordId}>"))
                        : "No players.",
                    IsInline = false
                }
            ]
        }.Build();
}