namespace GameMasterBot.Constants;

public static class CampaignCommands
{
    // Campaign Group
    public const string GroupName = "campaign";
    public const string GroupDescription = "Commands for managing the campaigns on your server.";
    
    // Create Campaign Command
    public const string CreateCommandName = "create";
    public const string CreateCommandDescription = "Creates a new campaign on this server, including new channels and roles.";
    public const string CreateCommandParamCampaignNameName = "campaign-name";
    public const string CreateCommandParamCampaignNameDescription = "The name of your new campaign.";
    public const string CreateCommandParamGameSystemName = "game-system";
    public const string CreateCommandParamGameSystemDescription = "The name of the game system you will use for your new campaign.";
    
    // Add Player Command
    public const string AddPlayerCommandName = "add-player";
    public const string AddPlayerCommandDescription = "Adds a new player to this campaign.";
    public const string AddPlayerCommandParamNewPlayerName = "new-player";
    public const string AddPlayerCommandParamNewPlayerDescription = "The person that you want to add as a player to this campaign.";

    // Remove Player Command
    public const string RemovePlayerCommandName = "remove-player";
    public const string RemovePlayerCommandDescription = "Removes a player from this campaign.";
    public const string RemovePlayerCommandParamPlayerToRemoveName = "player-to-remove";
    public const string RemovePlayerCommandParamPlayerToRemoveDescription = "The person that you want to remove as a player from this campaign.";

    // Set URL Command
    public const string SetUrlCommandName = "set-url";
    public const string SetUrlCommandDescription = "Sets the URL for this campaign where players can access the game online.";
    public const string SetUrlCommandParamGameUrlName = "game-url";
    public const string SetUrlCommandParamGameUrlDescription = "The URL where players can access the campaign.";

    // Set Game Master
    public const string SetGameMasterCommandName = "set-game-master";
    public const string SetGameMasterCommandDescription = "Assigns a new game master for this campaign.";
    public const string SetGameMasterCommandParamNewGameMasterName = "new-game-master";
    public const string SetGameMasterCommandParamNewGameMasterDescription = "The person that you want to assign as the new game master for this campaign.";

    // Campaign Delete Command
    public const string DeleteCommandName = "delete";
    public const string DeleteCommandDescription = "Deletes this campaign from the server, including channels and roles.";

    // Campaign View-Details Command
    public const string ViewDetailsCommandName = "view-details";
    public const string ViewDetailsCommandDescription = "Displays a brief overview of this campaign.";
}