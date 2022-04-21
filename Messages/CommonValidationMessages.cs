using GameMasterBot.Extensions;

namespace GameMasterBot.Messages;

public class CommonValidationMessages
{
    public static CommandValidationError NotInCampaignChannel() =>
        new("You must be in your campaign's text channel to use this command.");

    public static CommandValidationError NotGameMasterOrAdmin() =>
        new("You do not have permission to execute this command. You must either be the game master of this campaign or a server administrator.");
}