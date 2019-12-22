﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Interfaces.DataAccess;
using Common.Interfaces.Entities.Core;
using Discord.WebSocket;
using Entities.Core;
using GameMasterBot.Utils;

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
            session.Date = session.Schedule switch
            {
                "Daily" => session.Date.AddDays(1),
                "Weekly" => session.Date.AddDays(7),
                "BiWeekly" => session.Date.AddDays(14),
                "Monthly" => session.Date.AddMonths(1),
                _ => session.Date
            };
            if (session.Schedule != "AdHoc")
                await _unitOfWork.Sessions.Add(new Session
                {
                    CampaignId = session.CampaignId,
                    CampaignName = session.CampaignName,
                    ServerId = session.ServerId,
                    ServerName = session.ServerName,
                    ChannelId = session.ChannelId,
                    Schedule = session.Schedule,
                    Date = session.Date,
                    Expiry = DateUtils.ToUnixEpochTime(session.Date),
                    ReminderSent = false,
                    TriggerSent = false
                });
        }

        private async void CheckSessions(object state)
        {
            var sessions = _unitOfWork.Sessions.GetAllAfterDate(DateTime.UtcNow.AddMinutes(-35));
            foreach (var session in sessions)
            {
                var timeDiff = (session.Date - DateTime.UtcNow).TotalMinutes;
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
                    ? nextSession.Date.Subtract(DateTime.UtcNow).TotalSeconds - 1800
                    : nextSession.Date.Subtract(DateTime.UtcNow).TotalSeconds;
                if (nextTime < 0) nextTime = 0;
                if (nextTime > 86400) _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
                else _timer.Change(TimeSpan.FromSeconds(nextTime), TimeSpan.FromSeconds(nextTime));
            }
        }

        public async Task<ISession> Create(ulong channelId, ulong serverId, string serverName, string campaignId, string campaignName, string schedule, DateTime date)
        {
            var session = new Session
            {
                CampaignId = campaignId,
                CampaignName = campaignName,
                ServerId = serverId,
                ServerName = serverName,
                ChannelId = channelId,
                Schedule = schedule,
                Date = date,
                Expiry = DateUtils.ToUnixEpochTime(date),
                ReminderSent = false,
                TriggerSent = false
            };
            await _unitOfWork.Sessions.Add(session);
            SetTimerDelay();
            return session;
        }

        public async Task CancelNext(ulong serverId, string campaignId)
        {
            var nextSession = _unitOfWork.Sessions.GetForCampaignAfterDate(serverId, campaignId, DateTime.UtcNow).FirstOrDefault();
            if (nextSession == null)
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.Remove(nextSession);
            await CreateNextIfNecessary(nextSession);
            SetTimerDelay();
        }
        
        public async Task CancelForDay(ulong serverId, string campaignId, DateTime date)
        {
            var sessions = _unitOfWork.Sessions.GetForCampaignAfterDate(serverId, campaignId, date).ToList();
            if (!sessions.Any())
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.RemoveRange(sessions);
            foreach (var session in sessions) await CreateNextIfNecessary(session);
            SetTimerDelay();
        }

        public async Task CancelForPeriod(ulong serverId, string campaignId, DateTime after, DateTime before)
        {
            var sessions = _unitOfWork.Sessions.GetForCampaignForPeriod(serverId, campaignId, after, before).ToList();
            if (!sessions.Any())
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.RemoveRange(sessions);
            foreach (var session in sessions) await CreateNextIfNecessary(session);
            SetTimerDelay();
        }

        public async Task CancelForDayTime(ulong serverId, string campaignId, DateTime date)
        {
            var sessionSpecified = _unitOfWork.Sessions.GetForCampaign(serverId, campaignId).FirstOrDefault(session => session.Date == date);
            if (sessionSpecified == null)
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.Remove(sessionSpecified);
            await CreateNextIfNecessary(sessionSpecified);
            SetTimerDelay();
        }

        public async Task CancelSchedule(string campaignId, DateTime date)
        {
            var sessionSpecified = _unitOfWork.Sessions.GetAllAfterDate(date).FirstOrDefault(session => session.CampaignId == campaignId);
            if (sessionSpecified == null)
                throw new Exception("No sessions found for this campaign. Make sure you are either in a campaign channel or have specified the campaign.");
            await _unitOfWork.Sessions.Remove(sessionSpecified);
            SetTimerDelay();
        }

        public IEnumerable<ISession> GetUpcoming(ulong serverId, string campaignId) => _unitOfWork.Sessions.GetForCampaignAfterDate(serverId, campaignId, DateTime.UtcNow);
    }
}
