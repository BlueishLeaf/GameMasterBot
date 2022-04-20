using Discord.Interactions;

namespace GameMasterBot.Extensions
{
    public class CommandResult: RuntimeResult
    {
        private CommandResult(InteractionCommandError? error, string reason) : base(error, reason) { }

        public static CommandResult FromError(string reason) => new(InteractionCommandError.Unsuccessful, reason);
        
        public static CommandResult AsSuccess() => new(null, string.Empty);
    }
}
