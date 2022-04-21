namespace GameMasterBot.Messages;

public static class CampaignResponseMessages
{
    public static string CampaignSuccessfullyCreated(ulong textChannelId) =>
        $"Your campaign has been created successfully! You should head over to <#{textChannelId}> and start adding your players with the '/campaign add-player' command.";
}