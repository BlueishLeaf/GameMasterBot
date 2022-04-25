using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace GameMasterBot.Extensions
{
    public class RequireWhitelistedRoleOrAdminPrivilegesAttribute: PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is not SocketGuildUser gUser)
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
            if (gUser.Roles.Any(r => r.Name == "Whitelisted") || gUser.GuildPermissions.Administrator)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("You must have a role called 'Whitelisted' to run this command."));
        }
    }
}