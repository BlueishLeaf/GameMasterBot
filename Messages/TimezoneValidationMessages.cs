using GameMasterBot.Extensions;

namespace GameMasterBot.Messages;

public static class TimezoneValidationMessages
{
    public static CommandValidationError TimezoneNotFound() =>
        new("Sorry, I could not find your timezone. Please look for it under the 'TZ database name' column on this list https://en.wikipedia.org/wiki/List_of_tz_database_time_zones and try again.");
}