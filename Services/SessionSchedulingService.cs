using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMasterBot.Data;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameMasterBot.Services;

public class SessionSchedulingService(GameMasterBotContext context) : ISessionSchedulingService
{
    public async Task<List<Session>> GetAllUpcomingSessions() => 
        await context.Sessions.Where(s => s.State != SessionState.Archived).ToListAsync();

    public async Task<List<Session>> GetSessionsOccurringInNextMinutes(int minutes)
    {
        var nowPlusMinutes = DateTime.UtcNow.AddMinutes(minutes);
        return await context.Sessions
            .Where(s => s.Timestamp <= nowPlusMinutes && s.State != SessionState.Archived)
            .ToListAsync();
    }

    public async Task<Session> UpdateSession(Session session)
    {
        var sessionUpdated =context.Sessions.Update(session);
        await context.SaveChangesAsync();
        return sessionUpdated.Entity;
    }
    
    public async Task CreateNextIfNecessary(Session session)
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