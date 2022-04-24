using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMasterBot.Data;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameMasterBot.Services
{
    public class SessionService : ISessionService
    {
        private readonly GameMasterBotContext _context;
        private readonly SessionScheduler _sessionScheduler;

        public SessionService(GameMasterBotContext context, SessionScheduler sessionScheduler)
        {
            _context = context;
            _sessionScheduler = sessionScheduler;
        }

        private async Task CreateNextIfNecessary(Session session)
        {
            if (session.Frequency != ScheduleFrequency.Standalone)
            {
                var timestamp = session.Frequency switch
                {
                    ScheduleFrequency.Weekly => session.Timestamp.AddDays(7),
                    ScheduleFrequency.Fortnightly => session.Timestamp.AddDays(14),
                    ScheduleFrequency.Monthly => session.Timestamp.AddMonths(1),
                    _ => session.Timestamp
                };
                await _context.Sessions.AddAsync(new Session
                {
                    CampaignId = session.CampaignId,
                    Frequency = session.Frequency,
                    Timestamp = timestamp,
                    State = timestamp.Subtract(DateTime.UtcNow).TotalMinutes <= 30
                        ? SessionState.Confirmed
                        : SessionState.Scheduled
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Session> Create(long campaignId, ScheduleFrequency scheduleFrequency, DateTime timestamp)
        {
            var session = (await _context.Sessions.AddAsync(new Session
            {
                CampaignId = campaignId,
                Frequency = scheduleFrequency,
                Timestamp = timestamp,
                State = timestamp.Subtract(DateTime.UtcNow).TotalMinutes <= 30
                    ? SessionState.Confirmed
                    : SessionState.Scheduled
            })).Entity;
            await _context.SaveChangesAsync();
            await _sessionScheduler.RefreshTimerData();
            return session;
        }

        public async Task CancelNext(long campaignId)
        {
            var sessionFound = await _context.Sessions.FirstAsync(session =>
                session.Timestamp >= DateTime.UtcNow &&
                session.State != SessionState.Archived &&
                session.CampaignId == campaignId);
            sessionFound.State = SessionState.Archived;
            _context.Sessions.Update(sessionFound);
            await _context.SaveChangesAsync();
            await CreateNextIfNecessary(sessionFound);
            await _sessionScheduler.RefreshTimerData();
        }

        public async Task CancelRecurringById(long sessionId)
        {
            var sessionFound = await _context.Sessions.SingleAsync(s => s.Id == sessionId);
            sessionFound.State = SessionState.Archived;
            _context.Sessions.Update(sessionFound);
            await _context.SaveChangesAsync();
            await _sessionScheduler.RefreshTimerData();
        }

        public async Task<Session> GetRecurringByCampaignId(long campaignId) =>
            await _context.Sessions.FirstOrDefaultAsync(s =>
                s.CampaignId == campaignId &&
                s.State != SessionState.Archived &&
                s.Frequency != ScheduleFrequency.Standalone);

        public async Task<List<Session>> GetUpcomingByCampaignId(long campaignId) =>
            await _context.Sessions
                .Where(s =>
                    s.CampaignId == campaignId && s.Timestamp >= DateTime.UtcNow && s.State != SessionState.Archived)
                .ToListAsync();
    }
}

