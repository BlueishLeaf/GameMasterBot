using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using GameMasterBot.DTO;
using GameMasterBot.Extensions;

namespace GameMasterBot.Services;

public interface ICampaignValidationService
{
    Task<CommandValidationError> ValidateCreateCampaignCommand(SocketInteractionContext context, CreateSocketCampaignDto createSocketCampaignDto);
    Task<CommandValidationError> ValidateAddPlayerCommand(SocketInteractionContext context, IUser newPlayer);
    Task<CommandValidationError> ValidateRemovePlayerCommand(SocketInteractionContext context, IUser playerToRemove);
    Task<CommandValidationError> ValidateSetUrlCommand(SocketInteractionContext context, string url);
    Task<CommandValidationError> ValidateSetGameMaster(SocketInteractionContext context, IUser newGameMaster);
    Task<CommandValidationError> ValidateDeleteCampaignCommand(SocketInteractionContext context);
    Task<CommandValidationError> ValidateCampaignInfoCommand(SocketInteractionContext context);
}