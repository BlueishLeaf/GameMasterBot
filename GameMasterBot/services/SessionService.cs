using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Interfaces.DataAccess;
using Common.Interfaces.Entities.Core;
using Discord.WebSocket;
using Entities.Core;

namespace GameMasterBot.Services
{
    public class SessionService
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

        private async Task CreateNextIfNecessary(ISession session)
        {
            switch (session.Schedule)
            {
                case "Daily":
                    session.Date = session.Date.AddDays(1);
                    break;
                case "Weekly":
                    session.Date = session.Date.AddDays(7);
                    break;
                case "BiWeekly":
                    session.Date = session.Date.AddDays(14);
                    break;
                case "Monthly":
                    session.Date = session.Date.AddMonths(1);
                    break;
            }
            if (session.Schedule != "AdHoc")
                await _unitOfWork.Sessions.Add(new Session
                {
                    CampaignId = session.CampaignId,
                    ChannelId = session.ChannelId,
                    Schedule = session.Schedule,
                    Date = session.Date,
                    ReminderSent = false,
                    TriggerSent = false
                });
        }

        private async void CheckSessions(object state)
        {
            var sessions = _unitOfWork.Sessions.GetAllAfterDate(DateTime.UtcNow.AddMinutes(-35));
            foreach (var session in sessions)
            {
                var timeDiff = (session.Date - DateTime.Now).TotalMinutes;
                if (!(timeDiff <= 30) || session.ReminderSent && session.TriggerSent) continue;
                var channelToNotify = (SocketTextChannel)_client.GetChannel(session.ChannelId);
                if (timeDiff <= 0 && !session.TriggerSent)
                {
                    await channelToNotify.SendMessageAsync("@here Attention! Today's session is about to begin!");
                    session.TriggerSent = true;
                    await CreateNextIfNecessary(session);
                }
                else if (!session.ReminderSent)
                {
                    await channelToNotify.SendMessageAsync("@here Attention! Today's session will begin in ~30 minutes!");
                    session.ReminderSent = true;
                }
                await _unitOfWork.Sessions.Update(session);
            }
            SetTimerDelay();
        }

        private void SetTimerDelay()
        {
            var sessions = _unitOfWork.Sessions.GetAllAfterDate(DateTime.UtcNow.AddMinutes(-35)).ToList();
            if (sessions.FirstOrDefault(session => session.TriggerSent == false) == null) _timer.Change(Timeout.Infinite, Timeout.Infinite);
            else
            {
                var nextSession = sessions.OrderBy(session => session.Date).First(session => session.TriggerSent == false);
                var nextTime = !nextSession.ReminderSent
                    ? nextSession.Date.Subtract(DateTime.Now).TotalSeconds - 1800
                    : nextSession.Date.Subtract(DateTime.Now).TotalSeconds;
                if (nextTime < 0) nextTime = 0;
                if (nextTime > 86400) _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
                else _timer.Change(TimeSpan.FromSeconds(nextTime), TimeSpan.FromSeconds(nextTime));
            }
        }

        public async Task<ISession> Create(ulong channelId, ulong serverId, string campaignId,  string schedule, DateTime date)
        {
            var session = new Session
            {
                CampaignId = campaignId,
                ServerId = serverId,
                ChannelId = channelId,
                Schedule = schedule,
                Date = date,
                ReminderSent = false,
                TriggerSent = false
            };
            await _unitOfWork.Sessions.Add(session);
            SetTimerDelay();
            return session;
        }

        public async Task Cancel(string campaignId, bool deschedule)
        {
            var nextSession = _unitOfWork.Sessions.GetAllAfterDate(DateTime.UtcNow).FirstOrDefault(session => session.CampaignId == campaignId);
            if (nextSession == null)
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.Remove(nextSession);
            if (deschedule) await CreateNextIfNecessary(nextSession);
            SetTimerDelay();
        }

        public async Task CancelForDate(string campaignId, DateTime date, bool deschedule)
        {
            var sessions = _unitOfWork.Sessions.GetAllAfterDate(date).Where(session => session.CampaignId == campaignId).ToList();
            if (!sessions.Any())
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.RemoveRange(sessions);
            if (deschedule) foreach (var session in sessions) await CreateNextIfNecessary(session);
            SetTimerDelay();
        }

        public async Task CancelForDateTime(string campaignId, DateTime date, bool deschedule)
        {
            var sessionSpecified = _unitOfWork.Sessions.GetAllAfterDate(date).FirstOrDefault(session => session.CampaignId == campaignId);
            if (sessionSpecified == null)
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.Remove(sessionSpecified);
            if (deschedule) await CreateNextIfNecessary(sessionSpecified);
            SetTimerDelay();
        }

        public async Task DeSchedule(string campaignId)
        {
            var nextSession = _unitOfWork.Sessions.GetAllAfterDate(DateTime.UtcNow).FirstOrDefault(session => session.CampaignId == campaignId);
            if (nextSession == null)
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.Remove(nextSession);
            SetTimerDelay();
        }

        public ISession GetNext(string campaignId) => _unitOfWork.Sessions.GetAllAfterDate(DateTime.UtcNow).FirstOrDefault(session => session.CampaignId == campaignId);
    }
}
