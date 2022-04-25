using GameMasterBot.Extensions;

namespace GameMasterBot.Messages;

public static class SessionValidationMessages
{
    public static CommandValidationError InvalidDateTime() =>
        new("You entered an invalid date or time. The date must be in the form 'dd/MM/yyyy' and the time must be in the form 'hh:mm'.");
    
    public static CommandValidationError DateIsInPast() =>
        new("You cannot schedule a session in the past!");

    public static CommandValidationError NoUpcomingSessions() =>
        new("No upcoming sessions for this campaign have been scheduled yet.");
    
    public static CommandValidationError NoSessionsForTimestamp() =>
        new("There are no sessions scheduled for this campaign at that date/time.");

    public static CommandValidationError NoRecurringSessionsForTimestamp() =>
        new("You are no recurring sessions scheduled for this campaign at that date/time.");
}