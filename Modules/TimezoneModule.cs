using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Constants;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Services;
using GameMasterBot.Services.Interfaces;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group(TimezoneCommands.GroupName, TimezoneCommands.GroupDescription)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class TimezoneModule(IUserService userService) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand(TimezoneCommands.ViewCommandName, TimezoneCommands.ViewCommandDescription)]
        public async Task<RuntimeResult> TimezoneAsync()
        {
            var user = await userService.GetByDiscordUserId(Context.User.Id);

            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);

            await RespondAsync(TimezoneResponseMessages.CurrentlySetTimezone(tzInfo.Id), ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(TimezoneCommands.ViewAllCommandName, TimezoneCommands.ViewAllCommandDescription)]
        public async Task<RuntimeResult> ShowTimezonesAsync()
        {
            await RespondAsync(TimezoneResponseMessages.ListAllTimezones(), ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [SlashCommand(TimezoneCommands.SetCommandName, TimezoneCommands.SetCommandDescription)]
        public async Task<RuntimeResult> SetTimezoneAsync(
            [Summary(TimezoneCommands.SetCommandParamIanaTimezoneName, TimezoneCommands.SetCommandParamIanaTimezoneDescription)] string ianaTimezone)
        {
            var commandValidationError = TimezoneCommandValidator.ValidateSetTimezoneCommand(ianaTimezone);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone);
            await userService.UpdateTimezone(Context.User.Id, tzInfo.Id);

            await RespondAsync(TimezoneResponseMessages.SetNewTimezone(tzInfo.Id), ephemeral: true);
            return CommandResult.AsSuccess();
        }
    }
}