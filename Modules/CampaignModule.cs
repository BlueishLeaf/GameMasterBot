using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.Constants;
using GameMasterBot.DTOs;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Models.Entities;
using GameMasterBot.Services;
using GameMasterBot.Services.Interfaces;
using GameMasterBot.Utils;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group(CampaignCommands.GroupName, CampaignCommands.GroupDescription)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class CampaignModule(CampaignCommandValidator validator, ICampaignService campaignService, IUserService userService) : InteractionModuleBase<SocketInteractionContext>
    {
        [RequireWhitelistedRoleOrAdminPrivileges]
        [SlashCommand(CampaignCommands.CreateCommandName, CampaignCommands.CreateCommandDescription)]
        public async Task<RuntimeResult> CreateCampaignAsync(
            [Summary(CampaignCommands.CreateCommandParamCampaignNameName, CampaignCommands.CreateCommandParamCampaignNameDescription)] string campaignName,
            [Summary(CampaignCommands.CreateCommandParamGameSystemName, CampaignCommands.CreateCommandParamGameSystemDescription)] string gameSystem)
        {
            await DeferAsync(ephemeral: true);

            var createSocketCampaignDto = new CreateCampaignCommandDto(campaignName, gameSystem);

            var commandValidationError = await validator.ValidateCreateCampaignCommand(Context, createSocketCampaignDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var createCampaignDto = await CampaignSocketUtils.CreateSocketCampaign(Context, createSocketCampaignDto);
            var campaign = await campaignService.Create(createCampaignDto);

            await ModifyOriginalResponseAsync(message =>
            {
                message.Content = CampaignResponseMessages.CampaignSuccessfullyCreated(campaign.TextChannelId);
                message.Embed = CampaignEmbedBuilder.BuildCampaignEmbed(campaign);
            });
            return CommandResult.AsSuccess();
        }

        [RequireWhitelistedRoleOrAdminPrivileges]
        [SlashCommand(CampaignCommands.AddPlayerCommandName, CampaignCommands.AddPlayerCommandDescription)]
        public async Task<RuntimeResult> AddPlayerAsync(
            [Summary(CampaignCommands.AddPlayerCommandParamNewPlayerName, CampaignCommands.AddPlayerCommandParamNewPlayerDescription)] SocketGuildUser playerToAdd)
        {
            var commandValidationError = await validator.ValidateAddPlayerCommand(Context, playerToAdd);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await campaignService.AddPlayer(campaign.Id, playerToAdd.Id);
            
            await CampaignSocketUtils.AddPlayer(Context, playerToAdd, campaign.PlayerRoleId);
            
            await RespondAsync(CampaignResponseMessages.PlayerSuccessfullyAdded(playerToAdd.Id), embed: CampaignEmbedBuilder.BuildCampaignEmbed(campaign));
            return CommandResult.AsSuccess();
        }

        [RequireWhitelistedRoleOrAdminPrivileges]
        [SlashCommand(CampaignCommands.RemovePlayerCommandName, CampaignCommands.RemovePlayerCommandDescription)]
        public async Task<RuntimeResult> RemovePlayerAsync(
            [Summary(CampaignCommands.RemovePlayerCommandParamPlayerToRemoveName, CampaignCommands.RemovePlayerCommandParamPlayerToRemoveDescription)] SocketGuildUser playerToRemove)
        {
            var commandValidationError = await validator.ValidateRemovePlayerCommand(Context, playerToRemove);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await campaignService.RemovePlayer(campaign.Id, playerToRemove.Id);

            await CampaignSocketUtils.RemovePlayer(Context, playerToRemove, campaign.PlayerRoleId);

            await RespondAsync(CampaignResponseMessages.PlayerSuccessfullyRemoved(playerToRemove.Id), embed: CampaignEmbedBuilder.BuildCampaignEmbed(campaign), ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [RequireWhitelistedRoleOrAdminPrivileges]
        [SlashCommand(CampaignCommands.SetUrlCommandName, CampaignCommands.SetUrlCommandDescription)]
        public async Task<RuntimeResult> SetUrlAsync(
            [Summary(CampaignCommands.SetUrlCommandParamGameUrlName, CampaignCommands.SetUrlCommandParamGameUrlDescription)] string gameUrl)
        {
            var commandValidationError = await validator.ValidateSetUrlCommand(Context, gameUrl);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign.Url = gameUrl;
            campaign = await campaignService.Update(campaign);
            
            await RespondAsync(CampaignResponseMessages.UrlSuccessfullySet(), embed: CampaignEmbedBuilder.BuildCampaignEmbed(campaign), ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [RequireWhitelistedRoleOrAdminPrivileges]
        [SlashCommand(CampaignCommands.SetGameMasterCommandName, CampaignCommands.SetGameMasterCommandDescription)]
        public async Task<RuntimeResult> SetGameMasterAsync(
            [Summary(CampaignCommands.SetGameMasterCommandParamNewGameMasterName, CampaignCommands.SetGameMasterCommandParamNewGameMasterDescription)] SocketGuildUser newGameMaster)
        {
            var commandValidationError = await validator.ValidateSetGameMaster(Context, newGameMaster);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);

            await CampaignSocketUtils.SetGameMaster(Context, newGameMaster, campaign);
        
            var newGameMasterUser = await userService.GetByDiscordUserId(newGameMaster.Id);
            campaign.GameMaster = new GameMaster { User = newGameMasterUser };
            campaign = await campaignService.Update(campaign);
            
            await RespondAsync(CampaignResponseMessages.GameMasterSuccessfullySet(newGameMaster.Id), embed: CampaignEmbedBuilder.BuildCampaignEmbed(campaign));
            return CommandResult.AsSuccess();
        }

        [RequireWhitelistedRoleOrAdminPrivileges]
        [SlashCommand(CampaignCommands.DeleteCommandName, CampaignCommands.DeleteCommandDescription)]
        public async Task<RuntimeResult> DeleteCampaignAsync()
        {
            await DeferAsync(ephemeral: true);
            
            var commandValidationError = await validator.ValidateDeleteCampaignCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            
            await CampaignSocketUtils.DeleteCampaign(Context, campaign);
            
            await campaignService.DeleteById(campaign.Id);
            
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(CampaignCommands.ViewDetailsCommandName, CampaignCommands.ViewDetailsCommandDescription)]
        public async Task<RuntimeResult> ViewDetailsAsync()
        {
            var commandValidationError = await validator.ValidateCampaignInfoCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
                
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);

            await RespondAsync(embed: CampaignEmbedBuilder.BuildCampaignEmbed(campaign), ephemeral: true);
            return CommandResult.AsSuccess();
        }
    }
}
