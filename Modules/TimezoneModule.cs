using System;
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
    public class TimezoneModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TimezoneCommandValidator _validator;
        private readonly IUserService _userService;
        
        public TimezoneModule(TimezoneCommandValidator validator, IUserService userService)
        {
            _validator = validator;
            _userService = userService;
        }

        [SlashCommand(TimezoneCommands.ViewCommandName, TimezoneCommands.ViewCommandDescription)]
        public async Task<RuntimeResult> TimezoneAsync()
        {
            var user = await _userService.GetByDiscordUserId(Context.User.Id);

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
            var commandValidationError = _validator.ValidateSetTimezoneCommand(ianaTimezone);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone);
            await _userService.UpdateTimezone(Context.User.Id, tzInfo.Id);

            await RespondAsync(TimezoneResponseMessages.SetNewTimezone(tzInfo.Id), ephemeral: true);
            return CommandResult.AsSuccess();
        }
    }
}