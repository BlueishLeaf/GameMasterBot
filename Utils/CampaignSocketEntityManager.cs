using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using GameMasterBot.DTO;

namespace GameMasterBot.Utils;

public class CampaignSocketEntityManager
{
    private readonly SocketInteractionContext _context;

    public CampaignSocketEntityManager(SocketInteractionContext context) => _context = context;

    public async Task<CampaignEntitiesDto> CreateNewCampaign(string campaignName, string gameSystem)
    {
        var roleColor = RandomDiscordColor();

        var playerRole = _context.Guild.Roles.FirstOrDefault(role => role.Name == $"Player: {campaignName}") ??
                         (IRole)_context.Guild.CreateRoleAsync($"Player: {campaignName}", null, roleColor, false, true).Result;
        
        var gmRole = _context.Guild.Roles.FirstOrDefault(role => role.Name == $"Game Master: {campaignName}") ??
                         (IRole)_context.Guild.CreateRoleAsync($"Game Master: {campaignName}", null, roleColor, false, true).Result;
        
        // Create the category channel for this campaign's system if one does not already exist
        var campaignCategoryChannel = _context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == gameSystem) ??
                                          (ICategoryChannel)_context.Guild.CreateCategoryChannelAsync(gameSystem).Result;

        var textChannelName = campaignName.ToLower().Replace(' ', '-');

        // Create the text channel for this campaign if one does not exist
        var campaignTextChannel = _context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == textChannelName) ??
                                  (ITextChannel)_context.Guild.CreateTextChannelAsync(textChannelName, channel =>
                                  {
                                      channel.CategoryId = campaignCategoryChannel.Id;
                                      channel.Topic = $"Channel for discussing the {gameSystem} campaign '{campaignName}'.";
                                  }).Result;

        // Set the permissions on the campaign's text channel
        await campaignTextChannel.AddPermissionOverwriteAsync(_context.Guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny));
        await campaignTextChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow, addReactions: PermValue.Allow));
        await campaignTextChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow));

        // Create the voice channel for this campaign if one does not exist
        var campaignVoiceChannel = _context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == campaignName) ??
                                   (IVoiceChannel)_context.Guild.CreateVoiceChannelAsync(campaignName,
                                       channel => channel.CategoryId = campaignCategoryChannel.Id).Result;

        // Set the permissions on the campaign's voice channel
        await campaignVoiceChannel.AddPermissionOverwriteAsync(_context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
        await campaignVoiceChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
        await campaignVoiceChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));

        // Add the gm role to the game master
        await _context.Guild.Users.First(user => user.Id == _context.User.Id).AddRoleAsync(gmRole);

        return new CampaignEntitiesDto(campaignTextChannel.Id, campaignVoiceChannel.Id, playerRole.Id, gmRole.Id);
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