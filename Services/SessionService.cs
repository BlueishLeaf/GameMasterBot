﻿using System;
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
    public class SessionService(GameMasterBotContext context, SessionScheduler sessionScheduler) : ISessionService
    {
        public async Task<Session> Create(long campaignId, ScheduleFrequency scheduleFrequency, DateTime timestamp)
        {
            var session = (await context.Sessions.AddAsync(new Session
            {
                CampaignId = campaignId,
                Frequency = scheduleFrequency,
                Timestamp = timestamp,
                State = timestamp.Subtract(DateTime.UtcNow).TotalMinutes <= 30
                    ? SessionState.Confirmed
                    : SessionState.Scheduled
            })).Entity;
            await context.SaveChangesAsync();
            await sessionScheduler.RefreshTimerData();
            return session;
        }

        public async Task CancelNextByCampaignId(long campaignId)
        {
            var sessionsFound = await context.Sessions.Where(session =>
                session.Timestamp >= DateTime.UtcNow &&
                session.State != SessionState.Archived &&
                session.CampaignId == campaignId)
                .ToListAsync();
            var nextSession = sessionsFound.OrderBy(s => s.Timestamp).First();
            nextSession.State = SessionState.Archived;
            
            context.Sessions.Update(nextSession);
            await context.SaveChangesAsync();
            await CreateNextIfNecessary(nextSession);
            await sessionScheduler.RefreshTimerData();
        }

        public async Task CancelAllByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime)
        {
            var sessionsFound = await context.Sessions.Where(session =>
                session.CampaignId == campaignId &&
                session.Timestamp == utcDateTime &&
                session.State != SessionState.Archived)
                .ToListAsync();

            foreach (var session in sessionsFound)
            {
                session.State = SessionState.Archived;
                context.Sessions.Update(session);
                await CreateNextIfNecessary(session);
            }
            
            await context.SaveChangesAsync();
            await sessionScheduler.RefreshTimerData();   
        }

        public async Task CancelAllRecurringByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime)
        {
            var sessionsFound = await context.Sessions.Where(session =>
                    session.CampaignId == campaignId &&
                    session.Timestamp == utcDateTime &&
                    session.State != SessionState.Archived)
                .ToListAsync();

            foreach (var session in sessionsFound)
            {
                session.State = SessionState.Archived;
                context.Sessions.Update(session);
            }
            
            await context.SaveChangesAsync();
            await sessionScheduler.RefreshTimerData();   
        }

        public async Task CancelAllByCampaignId(long campaignId)
        {
            var sessionsFound = await context.Sessions.Where(session =>
                    session.CampaignId == campaignId &&
                    session.State != SessionState.Archived)
                .ToListAsync();

            foreach (var session in sessionsFound)
            {
                session.State = SessionState.Archived;
                context.Sessions.Update(session);
            }
            
            await context.SaveChangesAsync();
            await sessionScheduler.RefreshTimerData();   
        }

        public async Task<List<Session>> GetAllRecurringByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime) =>
            await context.Sessions.Where(s =>
                s.CampaignId == campaignId &&
                s.State != SessionState.Archived &&
                s.Frequency != ScheduleFrequency.Standalone &&
                s.Timestamp == utcDateTime)
                .ToListAsync();

        public async Task<List<Session>> GetAllUpcomingByCampaignId(long campaignId) =>
            await context.Sessions
                .Where(s =>
                    s.CampaignId == campaignId && s.Timestamp >= DateTime.UtcNow && s.State != SessionState.Archived)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();

        public async Task<List<Session>> GetAllByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime) =>
            await context.Sessions
                .Where(s =>
                    s.CampaignId == campaignId && s.Timestamp == utcDateTime && s.State != SessionState.Archived)
                .ToListAsync();

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
                Console.WriteLine($"{DateTime.Now:T} Creating new {session.Frequency} session for {timestamp:g}");
                await context.Sessions.AddAsync(new Session
                {
                    CampaignId = session.CampaignId,
                    Frequency = session.Frequency,
                    Timestamp = timestamp,
                    State = timestamp.Subtract(DateTime.UtcNow).TotalMinutes <= 30
                        ? SessionState.Confirmed
                        : SessionState.Scheduled
                });
                await context.SaveChangesAsync();
            }
        }
    }
}

