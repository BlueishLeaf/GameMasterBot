namespace GameMasterBot.Messages;

public static class SessionResponseMessages
{
    public static string SessionSuccessfullyScheduled() =>
        "Session successfully scheduled for this campaign!";
    
    public static string SessionJustScheduled() =>
        "Here are the details of the session that was just scheduled for this campaign.";

    public static string NextSessionScheduled() =>
        "Here are the details for the next session scheduled for this campaign.";

    public static string NextSessionCancelled() =>
        "Next session cancelled successfully.";
    
    public static string SessionCancelled() =>
        "Session cancelled successfully.";

    public static string NoMoreUpcomingSessions() =>
        "There are no more upcoming sessions for this campaign, you can schedule one with '/session schedule'.";

    public static string FollowingSessionDetails() =>
        "Here are the details of the next scheduled session for this campaign.";
    
    public static string RecurringSessionCancelled() =>
        "Recurring session cancelled successfully.";

    public static string AllSessionsCancelled() =>
        "All sessions for this campaign have been successfully cancelled.";
}