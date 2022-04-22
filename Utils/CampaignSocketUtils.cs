﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using GameMasterBot.DTO;

namespace GameMasterBot.Utils;

public static class CampaignSocketUtils
{
    public static async Task<CreateCampaignDto> CreateSocketCampaign(SocketInteractionContext context, CreateSocketCampaignDto createSocketCampaignDto)
    {
        var roleColor = RandomDiscordColor();

        var playerRole = context.Guild.Roles.FirstOrDefault(role => role.Name == $"Player: {createSocketCampaignDto.CampaignName}") ??
                         (IRole)context.Guild.CreateRoleAsync($"Player: {createSocketCampaignDto.CampaignName}", null, roleColor, false, true).Result;
        
        var gameMasterRole = context.Guild.Roles.FirstOrDefault(role => role.Name == $"Game Master: {createSocketCampaignDto.CampaignName}") ??
                         (IRole)context.Guild.CreateRoleAsync($"Game Master: {createSocketCampaignDto.CampaignName}", null, roleColor, false, true).Result;
        
        // Create the category channel for this campaign's system if one does not already exist
        var campaignCategoryChannel = context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == createSocketCampaignDto.GameSystem) ??
                                          (ICategoryChannel)context.Guild.CreateCategoryChannelAsync(createSocketCampaignDto.GameSystem).Result;

        var textChannelName = createSocketCampaignDto.CampaignName.ToLower().Replace(' ', '-');

        // Create the text channel for this campaign if one does not exist
        var campaignTextChannel = context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == textChannelName) ??
                                  (ITextChannel)context.Guild.CreateTextChannelAsync(textChannelName, channel =>
                                  {
                                      channel.CategoryId = campaignCategoryChannel.Id;
                                      channel.Topic = $"Channel for discussing the {createSocketCampaignDto.GameSystem} campaign '{createSocketCampaignDto.CampaignName}'.";
                                  }).Result;

        // Set the permissions on the campaign's text channel
        await campaignTextChannel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny));
        await campaignTextChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow, addReactions: PermValue.Allow));
        await campaignTextChannel.AddPermissionOverwriteAsync(gameMasterRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow));

        // Create the voice channel for this campaign if one does not exist
        var campaignVoiceChannel = context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == createSocketCampaignDto.CampaignName) ??
                                   (IVoiceChannel)context.Guild.CreateVoiceChannelAsync(createSocketCampaignDto.CampaignName,
                                       channel => channel.CategoryId = campaignCategoryChannel.Id).Result;

        // Set the permissions on the campaign's voice channel
        await campaignVoiceChannel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
        await campaignVoiceChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
        await campaignVoiceChannel.AddPermissionOverwriteAsync(gameMasterRole, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));

        // Add the gm role to the game master
        await context.Guild.Users.First(user => user.Id == context.User.Id).AddRoleAsync(gameMasterRole);

        return new CreateCampaignDto(
            createSocketCampaignDto.CampaignName,
            createSocketCampaignDto.GameSystem,
            context.User.Id,
            context.Guild.Id,
            campaignTextChannel.Id,
            campaignVoiceChannel.Id,
            playerRole.Id,
            gameMasterRole.Id);
    }

    private static Color RandomDiscordColor() =>
        new Random().Next(19) switch
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