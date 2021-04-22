using Discord.Commands;

namespace GameMasterBot.Extensions
{
    public class GameMasterResult: RuntimeResult
    {
        private GameMasterResult(CommandError? error, string reason) : base(error, reason) { }

        public static GameMasterResult ErrorResult(string reason) => new(CommandError.Unsuccessful, reason);

        public static GameMasterResult SuccessResult(string reason = "") => new(null, reason);
    }
}
