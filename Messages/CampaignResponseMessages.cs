namespace GameMasterBot.Messages;

public static class CampaignResponseMessages
{
    public static string CampaignSuccessfullyCreated(ulong textChannelId) =>
        $"Your campaign has been created successfully! You should head over to <#{textChannelId}> and start adding your players with the '/campaign add-player' command.";

    public static string PlayerSuccessfullyAdded(ulong newPlayerId) =>
        $"Successfully added <@{newPlayerId}> to this campaign as a new player!";

    public static string PlayerSuccessfullyRemoved(ulong removedPlayerId) =>
        $"Successfully removed <@{removedPlayerId}> from this campaign.";

    public static string UrlSuccessfullySet() =>
        "Successfully set the URL for this campaign.";
    
    public static string GameMasterSuccessfullySet(ulong newGameMasterId) =>
        $"Successfully set <@{newGameMasterId}> as the new game master for this campaign!";
}