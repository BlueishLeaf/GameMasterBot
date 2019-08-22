using System;
using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Services;
using GameMasterBot.Utils;
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [Group("session")]
    public class SessionModule: ModuleBase<SocketCommandContext>
    {
        private readonly SessionService _service;

        public SessionModule(SessionService service) => _service = service;

        [Command("add"), Summary("Creates a new session.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time)
        {
            if (!DateTime.TryParse($"{date} {time}", out var parsedDate))
                return GameMasterResult.ErrorResult("Invalid date.");

            var session = _service.Create(Context.Channel.Id, Context.Channel.Name, "AdHoc", parsedDate.ToUniversalTime()).Result;
            await ReplyAsync($"AdHoc session added for {session.Date}");
            return GameMasterResult.SuccessResult($"Session({date}-{time}) added successfully.");
        }
    }
}
