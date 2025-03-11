using System;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;

namespace GameMasterBot.Services;

public class TimezoneCommandValidator
{
    public static CommandValidationError ValidateSetTimezoneCommand(string ianaTimezone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimezoneValidationMessages.TimezoneNotFound();
        }

        return null;
    }
}