using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameMasterBot.Services
{
    public class SessionService
    {
        private readonly DiscordSocketClient _client;
        private readonly GameMasterBotContext _context;
        private readonly Timer _timer;
        private const int InitialDelay = 15;

        public SessionService(DiscordSocketClient client, GameMasterBotContext context)
        {
            _client = client;
            _context = context;
            _timer = new Timer(CheckSessions, null, TimeSpan.FromSeconds(InitialDelay),
                TimeSpan.FromSeconds(InitialDelay));
        }

        private async Task CreateNextIfNecessary(Session session)
        {
            if (session.ScheduleFrequency != ScheduleFrequency.Standalone)
            {
                var timestamp = session.ScheduleFrequency switch
                {
                    ScheduleFrequency.Weekly => session.Timestamp.AddDays(7),
                    ScheduleFrequency.Fortnightly => session.Timestamp.AddDays(14),
                    ScheduleFrequency.Monthly => session.Timestamp.AddMonths(1),
                    _ => session.Timestamp
                };
                await _context.Sessions.AddAsync(new Session
                {
                    CampaignId = session.CampaignId,
                    ScheduleFrequency = session.ScheduleFrequency,
                    Timestamp = timestamp,
                    State = timestamp.Subtract(DateTime.UtcNow).TotalMinutes <= 30
                        ? SessionState.Confirmed
                        : SessionState.Scheduled
                });
            }
        }

        private async void CheckSessions(object? state)
        {
            var sessions = await _context.Sessions.AsQueryable()
                .Where(s => s.Timestamp >= DateTime.UtcNow.AddMinutes(-35)).ToListAsync();
            foreach (var session in sessions)
            {
                var timeDiff = (session.Timestamp - DateTime.UtcNow).TotalMinutes;
                if (timeDiff > 30 || session.State == SessionState.Archived) continue;
                var channelToNotify = (SocketTextChannel) _client.GetChannel(session.Campaign.TextChannelId);
                if (timeDiff <= 0 && session.State == SessionState.Confirmed)
                {
                    await channelToNotify.SendMessageAsync($"<@&{session.Campaign.GameMasterRoleId}>, <@&{session.Campaign.PlayerRoleId}> Attention! Today's session is about to begin!");
                    session.State = SessionState.Archived;
                    await CreateNextIfNecessary(session);
                }
                else if (session.State == SessionState.Scheduled)
                {
                    await channelToNotify.SendMessageAsync($"<@&{session.Campaign.GameMasterRoleId}>, <@&{session.Campaign.PlayerRoleId}> Attention! Today's session will begin in ~30 minutes!");
                    session.State = SessionState.Confirmed;
                }

                _context.Sessions.Update(session);
                await _context.SaveChangesAsync();
            }

            SetTimerDelay(sessions);
        }

        private void SetTimerDelay(IReadOnlyCollection<Session> sessions)
        {
            if (sessions.FirstOrDefault(s => s.State != SessionState.Archived) == null)
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            else
            {
                var nextSession = sessions.OrderBy(s => s.Timestamp)
                    .First(session => session.State != SessionState.Archived);
                var nextTime = nextSession.State == SessionState.Scheduled
                    ? nextSession.Timestamp.Subtract(DateTime.UtcNow).TotalSeconds - 1800
                    : nextSession.Timestamp.Subtract(DateTime.UtcNow).TotalSeconds;
                if (nextTime < 0) nextTime = 0;
                if (nextTime > 86400) _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
                else _timer.Change(TimeSpan.FromSeconds(nextTime), TimeSpan.FromSeconds(nextTime));
            }
        }

        private async Task RefreshTimerData() =>
            SetTimerDelay(await _context.Sessions.AsQueryable()
                .Where(s => s.Timestamp >= DateTime.UtcNow.AddMinutes(-35)).ToListAsync());

        public async Task<Session> Create(long campaignId, ScheduleFrequency scheduleFrequency, DateTime timestamp)
        {
            var session = (await _context.Sessions.AddAsync(new Session
            {
                CampaignId = campaignId,
                ScheduleFrequency = scheduleFrequency,
                Timestamp = timestamp,
                State = timestamp.Subtract(DateTime.UtcNow).TotalMinutes <= 30
                    ? SessionState.Confirmed
                    : SessionState.Scheduled
            })).Entity;
            await _context.SaveChangesAsync();
            await RefreshTimerData();
            return session;
        }

        public async Task CancelNext(long campaignId)
        {
            var session = await _context.Sessions.AsQueryable().FirstOrDefaultAsync(s =>
                s.Timestamp >= DateTime.UtcNow && s.State != SessionState.Archived && s.CampaignId == campaignId);
            if (session == null) throw new Exception("no sessions found for this campaign.");
            session.State = SessionState.Archived;
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
            await CreateNextIfNecessary(session);
            await RefreshTimerData();
        }

        public async Task CancelForDate(long campaignId, DateTime date)
        {
            var session = await _context.Sessions.AsQueryable().FirstOrDefaultAsync(s =>
                s.Timestamp >= date && s.Timestamp < date.AddDays(1) && s.State != SessionState.Archived &&
                s.CampaignId == campaignId);
            if (session == null) throw new Exception("no sessions are scheduled on that date for this campaign.");
            session.State = SessionState.Archived;
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
            await CreateNextIfNecessary(session);
            await RefreshTimerData();
        }

        public async Task CancelScheduleForDate(long campaignId, DateTime date)
        {
            var session = await _context.Sessions.AsQueryable().FirstOrDefaultAsync(s =>
                s.Timestamp >= date && s.Timestamp < date.AddDays(1) && s.State != SessionState.Archived &&
                s.CampaignId == campaignId);
            if (session == null) throw new Exception("no sessions are scheduled for this campaign.");
            session.State = SessionState.Archived;
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
            await RefreshTimerData();
        }

        public async Task<List<Session>> GetUpcoming(long campaignId) =>
            await _context.Sessions.AsQueryable().Where(s =>
                    s.CampaignId == campaignId && s.Timestamp >= DateTime.Now && s.State != SessionState.Archived)
                .ToListAsync();
    }
}

