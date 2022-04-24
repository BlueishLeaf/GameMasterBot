using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.DTOs;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Utils;

public static class CampaignSocketUtils
{
    public static async Task<CreateCampaignDto> CreateSocketCampaign(SocketInteractionContext context, CreateCampaignCommandDto createCampaignCommandDto)
    {
        var roleColor = RandomDiscordColor();

        var playerRole = context.Guild.Roles.FirstOrDefault(role => role.Name == $"Player: {createCampaignCommandDto.CampaignName}") ??
                         (IRole)context.Guild.CreateRoleAsync($"Player: {createCampaignCommandDto.CampaignName}", null, roleColor, false, true).Result;
        
        var gameMasterRole = context.Guild.Roles.FirstOrDefault(role => role.Name == $"Game Master: {createCampaignCommandDto.CampaignName}") ??
                         (IRole)context.Guild.CreateRoleAsync($"Game Master: {createCampaignCommandDto.CampaignName}", null, roleColor, false, true).Result;
        
        // Create the category channel for this campaign's system if one does not already exist
        var campaignCategoryChannel = context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == createCampaignCommandDto.GameSystem) ??
                                          (ICategoryChannel)context.Guild.CreateCategoryChannelAsync(createCampaignCommandDto.GameSystem).Result;

        var textChannelName = createCampaignCommandDto.CampaignName.ToLower().Replace(' ', '-');

        // Create the text channel for this campaign if one does not exist
        var campaignTextChannel = context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == textChannelName) ??
                                  (ITextChannel)context.Guild.CreateTextChannelAsync(textChannelName, channel =>
                                  {
                                      channel.CategoryId = campaignCategoryChannel.Id;
                                      channel.Topic = $"Channel for discussing the {createCampaignCommandDto.GameSystem} campaign '{createCampaignCommandDto.CampaignName}'.";
                                  }).Result;

        // Set the permissions on the campaign's text channel
        await campaignTextChannel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny));
        await campaignTextChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow, addReactions: PermValue.Allow, embedLinks: PermValue.Allow));
        await campaignTextChannel.AddPermissionOverwriteAsync(gameMasterRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageMessages: PermValue.Allow, addReactions: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow, embedLinks: PermValue.Allow));

        // Create the voice channel for this campaign if one does not exist
        var campaignVoiceChannel = context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == createCampaignCommandDto.CampaignName) ??
                                   (IVoiceChannel)context.Guild.CreateVoiceChannelAsync(createCampaignCommandDto.CampaignName,
                                       channel => channel.CategoryId = campaignCategoryChannel.Id).Result;

        // Set the permissions on the campaign's voice channel
        await campaignVoiceChannel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
        await campaignVoiceChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
        await campaignVoiceChannel.AddPermissionOverwriteAsync(gameMasterRole, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));

        // Add the gm role to the game master
        await context.Guild.Users.First(user => user.Id == context.User.Id).AddRoleAsync(gameMasterRole);

        return new CreateCampaignDto(
            createCampaignCommandDto.CampaignName,
            createCampaignCommandDto.GameSystem,
            context.User.Id,
            context.Guild.Id,
            campaignTextChannel.Id,
            campaignVoiceChannel.Id,
            playerRole.Id,
            gameMasterRole.Id);
    }

    private static Color RandomDiscordColor()
    {
        return new Random().Next(19) switch
        {
            0 => Color.Blue,
            1 => Color.Green,
            2 => Color.Purple,
            3 => Color.Orange,
            4 => Color.Red,
            5 => Color.Teal,
            6 => Color.Gold,
            7 => Color.Magenta,
            8 => Color.DarkBlue,
            9 => Color.DarkerGrey,
            10 => Color.DarkGreen,
            11 => Color.DarkGrey,
            12 => Color.DarkMagenta,
            13 => Color.DarkOrange,
            14 => Color.DarkPurple,
            15 => Color.DarkRed,
            16 => Color.DarkTeal,
            17 => Color.LighterGrey,
            18 => Color.LightGrey,
            19 => Color.LightOrange,
            _ => Color.Default
        };
    }

    public static async Task AddPlayer(SocketInteractionContext context, SocketGuildUser newPlayer, ulong playerRoleId)
    {
        var campaignRole = context.Guild.Roles.First(role => role.Id == playerRoleId);
        await newPlayer.AddRoleAsync(campaignRole);
    }
    
    public static async Task RemovePlayer(SocketInteractionContext context, SocketGuildUser playerToRemove, ulong playerRoleId)
    {
        var campaignRole = context.Guild.Roles.First(role => role.Id == playerRoleId);
        await playerToRemove.RemoveRoleAsync(campaignRole);
    }

    public static async Task SetGameMaster(SocketInteractionContext context, SocketGuildUser newGameMaster, Campaign campaign)
    {
        var gmRole = context.Guild.Roles.First(r => r.Id == campaign.GameMasterRoleId);
        var currentGmDiscord = context.Guild.GetUser(campaign.GameMaster.User.DiscordId);
        if (currentGmDiscord != null) await currentGmDiscord.RemoveRoleAsync(gmRole);

        var playerRole = context.Guild.Roles.First(r => r.Id == campaign.PlayerRoleId);
        var player = campaign.Players.SingleOrDefault(cu => cu.User.DiscordId == newGameMaster.Id);
        if (player != null)
        {
            await newGameMaster.RemoveRoleAsync(playerRole);
            await newGameMaster.AddRoleAsync(gmRole);
            campaign.Players.Remove(player);
        }
        else await newGameMaster.AddRoleAsync(gmRole);
    }

    public static async Task DeleteCampaign(SocketInteractionContext context, Campaign campaign)
    {
        var textChannel = context.Guild.TextChannels.FirstOrDefault(channel => channel.Id == campaign.TextChannelId);
        if (textChannel != null) await context.Guild.GetTextChannel(textChannel.Id).DeleteAsync();

        var voiceChannel = context.Guild.VoiceChannels.FirstOrDefault(channel => channel.Id == campaign.VoiceChannelId);
        if (voiceChannel != null) await context.Guild.GetVoiceChannel(voiceChannel.Id).DeleteAsync();

        var campaignRole = context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
        if (campaignRole != null) await context.Guild.GetRole(campaignRole.Id).DeleteAsync();

        var gmRole = context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.GameMasterRoleId);
        if (gmRole != null) await context.Guild.GetRole(gmRole.Id).DeleteAsync();
    }
}