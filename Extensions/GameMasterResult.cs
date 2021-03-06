﻿using Discord.Commands;

namespace GameMasterBot.Extensions
{
    public class GameMasterResult: RuntimeResult
    {
        private GameMasterResult(CommandError? error, string reason) : base(error, reason) { }

        public static GameMasterResult ErrorResult(string reason) => new GameMasterResult(CommandError.Unsuccessful, reason);

        public static GameMasterResult SuccessResult(string reason = "") => new GameMasterResult(null, reason);
    }
}
