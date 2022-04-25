namespace GameMasterBot.Messages;

public static class TimezoneResponseMessages
{
    public static string CurrentlySetTimezone(string timezoneId) =>
        $"Your timezone is currently set to '{timezoneId}'. If your timezone is incorrect, you can use '/timezone set' to set the correct one.";

    public static string ListAllTimezones() =>
        "View timezones compatible with '/set-timezone' here: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones";
    
    public static string SetNewTimezone(string timezoneId) =>
        $"Successfully set your timezone to {timezoneId}.";
}