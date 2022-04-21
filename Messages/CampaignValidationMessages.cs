using GameMasterBot.Constants;
using GameMasterBot.Extensions;

namespace GameMasterBot.Messages;

public static class CampaignValidationMessages
{
    public static CommandValidationError InvalidNameLength() =>
        new($"The name of your campaign must be less than {CampaignValidationConstants.NameMaxLength} characters.");

    public static CommandValidationError InvalidNamePattern() =>
        new("The name of your campaign must only contain alphanumeric characters and spaces.");

    public static CommandValidationError InvalidSystemLength() =>
        new($"The name of your campaign's game system must be less than {CampaignValidationConstants.NameMaxLength} characters long.");
    
    public static CommandValidationError InvalidSystemPattern() =>
        new("The name of your game system must only contain alphanumeric characters and spaces.");

    public static CommandValidationError CampaignAlreadyExists() =>
        new("A campaign with this name already exists on this server.");
}