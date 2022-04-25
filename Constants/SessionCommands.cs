namespace GameMasterBot.Constants;

public static class SessionCommands
{
    // Session Group
    public const string GroupName = "session";
    public const string GroupDescription = "Commands for managing the sessions of a campaign.";
    
    // Session Schedule Command
    public const string ScheduleCommandName = "schedule";
    public const string ScheduleCommandDescription = "Schedules a new recurring or stanadalone session for a campaign.";
    public const string ScheduleCommandParamDateName = "date";
    public const string ScheduleCommandParamDateDescription = "The date on which the session will take place. Must be in the form 'dd/MM/yyyy'.";
    public const string ScheduleCommandParamTimeName = "time";
    public const string ScheduleCommandParamTimeDescription = "The time at which the session will take place. Must be in the form 'hh:mm'.";
    public const string ScheduleCommandParamFrequencyName = "frequency";
    public const string ScheduleCommandParamFrequencyDescription = "How often this session should re-occur. Can also be standalone.";
    public const string ScheduleCommandParamFrequencyChoiceStandaloneName = "Standalone";
    public const int ScheduleCommandParamFrequencyChoiceStandaloneValue = 0;
    public const string ScheduleCommandParamFrequencyChoiceWeeklyName = "Weekly";
    public const int ScheduleCommandParamFrequencyChoiceWeeklyValue = 1;
    public const string ScheduleCommandParamFrequencyChoiceFortnightlyName = "Fortnightly";
    public const int ScheduleCommandParamFrequencyChoiceFortnightlyValue = 2;
    public const string ScheduleCommandParamFrequencyChoiceMonthlyName = "Monthly";
    public const int ScheduleCommandParamFrequencyChoiceMonthlyValue = 3;
    
    // Session View-Next Command
    public const string ViewNextCommandName = "view-next";
    public const string ViewNextCommandDescription = "Displays the details of the next scheduled session for this campaign.";
    
    // Session View-Upcoming Command
    public const string ViewUpcomingCommandName = "view-upcoming";
    public const string ViewUpcomingCommandDescription = "Displays the details of all the currently scheduled sessions for this campaign.";
    
    // Session Cancel Command
    public const string CancelCommandName = "cancel";
    public const string CancelCommandDescription = "Cancels a scheduled session at the specified date/time for this campaign.";
    public const string CancelCommandParamDateName = "date";
    public const string CancelCommandParamDateDescription = "The date of the session you want to cancel. Must be in the form 'dd/MM/yyyy'.";
    public const string CancelCommandParamTimeName = "time";
    public const string CancelCommandParamTimeDescription = "The time of the session you want to cancel. Must be in the form 'hh:mm'.";
    
    // Session Cancel-Next Command
    public const string CancelNextCommandName = "cancel-next";
    public const string CancelNextCommandDescription = "Cancels the next scheduled session for this campaign.";
    
    // Session Cancel-Recurring Command
    public const string CancelRecurringCommandName = "cancel-recurring";
    public const string CancelRecurringCommandDescription = "Cancels a scheduled recurring session at the specified date/time for this campaign and stops it from re-occuring.";
    public const string CancelRecurringCommandParamDateName = "date";
    public const string CancelRecurringCommandParamDateDescription = "The date of the session you want to cancel. Must be in the form 'dd/MM/yyyy'.";
    public const string CancelRecurringCommandParamTimeName = "time";
    public const string CancelRecurringCommandParamTimeDescription = "The time of the session you want to cancel. Must be in the form 'hh:mm'.";
    
    // Session Cancel-All Command
    public const string CancelAllCommandName = "cancel-all";
    public const string CancelAllCommandDescription = "Cancels all scheduled sessions for this campaign and stops and recurring sessions from re-occuring.";
}