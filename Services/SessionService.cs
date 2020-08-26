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
    private readonly GameMasterContext _context;
    private readonly Timer _timer;
    
    private const int InitialDelay = 15;
    
    public SessionService(DiscordSocketClient client, GameMasterContext context)
    {
        _client = client;
        _context = context;
        _timer = new Timer(CheckSessions, null, TimeSpan.FromSeconds(InitialDelay), TimeSpan.FromSeconds(InitialDelay));
    }
    
    private async Task CreateNextIfNecessary(Session session)
    {
        if (session.Schedule != Schedule.AdHoc)
        {
            var timestamp = session.Schedule switch
            {
                Schedule.Weekly => session.Timestamp.AddDays(7),
                Schedule.Fortnightly => session.Timestamp.AddDays(14),
                Schedule.Monthly => session.Timestamp.AddMonths(1),
                _ => session.Timestamp
            };
            await _context.Sessions.AddAsync(new Session
            {
                CampaignId = session.CampaignId,
                Schedule = session.Schedule,
                Timestamp = timestamp,
            });
        }
    }
    
    private async void CheckSessions(object state)
    {
        var sessions = await _context.Sessions.Where(s => s.Timestamp >= DateTime.UtcNow.AddMinutes(-35)).ToListAsync();
        foreach (var session in sessions)
        {
            var timeDiff = (session.Timestamp - DateTime.UtcNow).TotalMinutes;
            if (!(timeDiff <= 30) || session.ReminderSent && session.Activated) continue;
            var channelToNotify = (SocketTextChannel)_client.GetChannel(session.Campaign.TextChannelId);
            if (timeDiff <= 0 && !session.Activated)
            {
                await channelToNotify.SendMessageAsync("@here Attention! Today's session is about to begin!");
                session.Activated = true;
                await CreateNextIfNecessary(session);
            }
            else if (!session.ReminderSent)
            {
                await channelToNotify.SendMessageAsync("@here Attention! Today's session will begin in ~30 minutes!");
                session.ReminderSent = true;
            }
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
        }
        SetTimerDelay(sessions);
    }
    
    private void SetTimerDelay(IReadOnlyCollection<Session> sessions)
    {
        if (sessions.FirstOrDefault(s => !s.Activated) == null) _timer.Change(Timeout.Infinite, Timeout.Infinite);
        else
        {
            var nextSession = sessions.OrderBy(s => s.Timestamp).First(session => !session.Activated);
            var nextTime = !nextSession.ReminderSent
                ? nextSession.Timestamp.Subtract(DateTime.UtcNow).TotalSeconds - 1800
                : nextSession.Timestamp.Subtract(DateTime.UtcNow).TotalSeconds;
            if (nextTime < 0) nextTime = 0;
            if (nextTime > 86400) _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
            else _timer.Change(TimeSpan.FromSeconds(nextTime), TimeSpan.FromSeconds(nextTime));
        }
    }

    private async Task RefreshTimerData() => 
        SetTimerDelay(await _context.Sessions.Where(s => s.Timestamp >= DateTime.UtcNow.AddMinutes(-35)).ToListAsync());

    public async Task<Session> Create(ulong campaignId, Schedule schedule, DateTime timestamp)
    {
        var session = (await _context.Sessions.AddAsync(new Session
        {
            CampaignId = campaignId,
            Schedule = schedule,
            Timestamp = timestamp
        })).Entity;
        await _context.SaveChangesAsync();
        await RefreshTimerData();
        return session;
    }
        
    public async Task CancelNext(ulong campaignId)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Timestamp >= DateTime.UtcNow && !s.Activated && s.CampaignId == campaignId);
        if (session == null) throw new Exception("no sessions found for this campaign.");
        session.Activated = true;
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        await CreateNextIfNecessary(session);
        await RefreshTimerData();
    }
    
    public async Task CancelForDate(ulong campaignId, DateTime date)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Timestamp >= date && s.Timestamp < date.AddDays(1) && !s.Activated && s.CampaignId == campaignId);
        if (session == null) throw new Exception("no sessions are scheduled on that date for this campaign.");
        session.Activated = true;
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        await CreateNextIfNecessary(session);
        await RefreshTimerData();
    }
    
    public async Task CancelScheduleForDate(ulong campaignId, DateTime date)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Timestamp >= date && s.Timestamp < date.AddDays(1) && !s.Activated && s.CampaignId == campaignId);
        if (session == null) throw new Exception("no sessions are scheduled for this campaign.");
        session.Activated = true;
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        await RefreshTimerData();
    }
    
    public async Task<List<Session>> GetUpcoming(ulong campaignId) =>
        await _context.Sessions.Where(s => s.CampaignId == campaignId && s.Timestamp >= DateTime.Now && !s.Activated).ToListAsync();
    }
}
