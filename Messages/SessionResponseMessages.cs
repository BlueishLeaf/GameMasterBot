namespace GameMasterBot.Messages;

public static class SessionResponseMessages
{
    public static string SessionSuccessfullyScheduled() =>
        "Session successfully scheduled for this campaign!";
    
    public static string SessionJustScheduled(string campaignName) =>
        $"Here are the details of the session that was just scheduled for {campaignName}.";

    public static string NextSessionScheduled(string campaignName) =>
        $"Here are the details for the next session scheduled for {campaignName}.";

    public static string NextSessionCancelled() =>
        "Next session cancelled successfully.";

    public static string NoMoreUpcomingSessions(string campaignName) =>
        $"There are no more upcoming sessions for {campaignName}, you can schedule one with '/session schedule'.";

    public static string FollowingSessionDetails(string campaignName) =>
        $"Here are the details of the next scheduled session for {campaignName}.";
    
    public static string RecurringSessionCancelled() =>
        "Recurring session cancelled successfully.";
}