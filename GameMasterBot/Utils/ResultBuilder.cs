using Discord.Commands;

namespace GameMasterBot.Utils
{
    public class ResultBuilder: RuntimeResult
    {
        public ResultBuilder(CommandError? error, string reason) : base(error, reason) { }

        public static ResultBuilder ErrorResult(string reason) => new ResultBuilder(CommandError.Unsuccessful, reason);

        public static ResultBuilder SuccessResult(string reason) => new ResultBuilder(null, reason);
    }
}
