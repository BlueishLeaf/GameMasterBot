namespace GameMasterBot.Constants;

public static class TimezoneCommands
{
    // Timezone Group
    public const string GroupName = "timezone";
    public const string GroupDescription = "Commands for managing users' timezones.";
    
    // Timezone View
    public const string ViewCommandName = "view";
    public const string ViewCommandDescription = "Displays the timezone that you have set for yourself.";
    
    // Timezone View-All
    public const string ViewAllCommandName = "view-all";
    public const string ViewAllCommandDescription = "Displays all timezones compatible with this bot.";
    
    // Timezone Set
    public const string SetCommandName = "set";
    public const string SetCommandDescription = "Sets your timezone to make scheduling sessions easier.";
    public const string SetCommandParamIanaTimezoneName = "iana-timezone";
    public const string SetCommandParamIanaTimezoneDescription = "Your IANA timezone (case-sensitive). Use '/timezone view-all' to find your IANA timezone.";
}