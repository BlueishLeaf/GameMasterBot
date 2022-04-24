using GameMasterBot.Extensions;

namespace GameMasterBot.Messages;

public static class SessionValidationMessages
{
    public static CommandValidationError InvalidDateTime() =>
        new("You entered an invalid date or time. The date must be in the form 'dd/MM/yyyy' and the time must be in the form 'hh:mm'.");
    
    public static CommandValidationError DateIsInPast() =>
        new("You cannot schedule a session in the past!");

    public static CommandValidationError RecurringSessionAlreadyExists() =>
        new("You cannot schedule another recurring session before removing your existing one with '/session cancel-schedule'");

    public static CommandValidationError NoUpcomingSessions() =>
        new("The next session for this campaign has not been scheduled yet.");

    public static CommandValidationError NoRecurringSessions() =>
        new("You do not have a recurring session scheduled for this campaign.");
}