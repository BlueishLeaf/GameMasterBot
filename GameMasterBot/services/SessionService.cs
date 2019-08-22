using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Interfaces.DataAccess;
using Common.Interfaces.Entities.Core;
using Discord.Commands;
using Discord.WebSocket;
using Entities.Core;

namespace GameMasterBot.Services
{
    public class SessionService: ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IUnitOfWork _unitOfWork;
        private Timer _timer;

        private const int InitialDelay = 10;

        public SessionService(DiscordSocketClient client, IUnitOfWork unitOfWork)
        {
            _client = client;
            _unitOfWork = unitOfWork;
        }

        public void Initialize() => SetTimer();

        private void SetTimer() => _timer = new Timer(CheckSessions, null, TimeSpan.FromSeconds(InitialDelay), TimeSpan.FromSeconds(InitialDelay));

        private async void CheckSessions(object state)
        {
            var sessions = _unitOfWork.Sessions.GetAllUpcoming();
            foreach (var session in sessions)
            {
                var timeUntilStart = session.Date.CompareTo(DateTime.UtcNow);
                if (timeUntilStart > 0) continue;
                var channelToNotify = (SocketTextChannel) _client.GetChannel(session.ChannelId);
                await channelToNotify.SendMessageAsync("@here Attention! Today's session is about to begin!");
                // _unitOfWork.Sessions.Remove(session.CampaignId, session.Date);
                switch (session.Schedule)
                {
                    case "Weekly":
                        session.Date = session.Date.AddDays(7);
                        break;
                    case "BiWeekly":
                        session.Date = session.Date.AddDays(14);
                        break;
                    case "Monthly":
                        session.Date = session.Date.AddMonths(1);
                        break;
                    case "AdHoc":
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (session.Schedule != "AdHoc")
                    await _unitOfWork.Sessions.Add(new Session
                    {
                        CampaignId = session.CampaignId,
                        ChannelId = session.ChannelId,
                        Schedule = session.Schedule,
                        Date = session.Date
                    });
            }
            SetTimerDelay();
        }

        private void SetTimerDelay()
        {
            var sessions = _unitOfWork.Sessions.GetAllUpcoming().ToList();
            if (!sessions.Any()) _timer.Change(Timeout.Infinite, Timeout.Infinite);
            else
            {
                var ordered = sessions.OrderBy(session => session.Date);
                var nextTime = ordered.First().Date.Subtract(DateTime.UtcNow).TotalSeconds;
                if (nextTime < 0) nextTime = 0;
                if (nextTime > 86400) _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
                else _timer.Change(TimeSpan.FromSeconds(nextTime), TimeSpan.FromSeconds(nextTime));
            }
        }

        public async Task<ISession> Create(ulong channelId, string campaignId,  string schedule, DateTime date)
        {
            var session = new Session
            {
                CampaignId = campaignId,
                ChannelId = channelId,
                Schedule = schedule,
                Date = date
            };
            await _unitOfWork.Sessions.Add(session);
            SetTimerDelay();
            return session;
        }
    }
}
