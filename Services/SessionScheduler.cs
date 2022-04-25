using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services.Interfaces;

namespace GameMasterBot.Services;

public class SessionScheduler
{
    private readonly DiscordSocketClient _client;
    private readonly ISessionSchedulingService _sessionSchedulingService;
    private readonly Timer _timer;
    private const int InitialDelay = 10;
    private const int TimerSessionReminderDelaySeconds = 1800;
    private const int SessionStartReminderWindowMinutes = 30;

    public SessionScheduler(DiscordSocketClient client, ISessionSchedulingService sessionSchedulingService)
    {
        _client = client;
        _sessionSchedulingService = sessionSchedulingService;
        _timer = new Timer(
            CheckSessions, 
            null,
            TimeSpan.FromSeconds(InitialDelay), 
            TimeSpan.FromSeconds(InitialDelay));
    }
    
    public async Task RefreshTimerData()
    {
        var sessions = await _sessionSchedulingService.GetAllUpcomingSessions();
        SetTimerDelay(sessions);
    }

    private void SetTimerDelay(IReadOnlyCollection<Session> sessions)
    {
        if (sessions.FirstOrDefault(s => s.State != SessionState.Archived) == null)
        {
            Console.WriteLine($"{DateTime.Now:T} No active sessions, setting infinite timeout...");
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        else
        {
            var nextSession = sessions.OrderBy(s => s.Timestamp)
                .First(session => session.State != SessionState.Archived);
            
            var nextTime = nextSession.State == SessionState.Scheduled
                ? nextSession.Timestamp.Subtract(DateTime.UtcNow).TotalSeconds - TimerSessionReminderDelaySeconds
                : nextSession.Timestamp.Subtract(DateTime.UtcNow).TotalSeconds;
            if (nextTime < 0) nextTime = 0;

            _timer.Change(TimeSpan.FromSeconds(nextTime), TimeSpan.FromSeconds(nextTime));
            Console.WriteLine($"{DateTime.Now:T} Timer delay set to {(int) nextTime} seconds");
        }
    }

    private async void CheckSessions(object state)
    {
        var sessions = await _sessionSchedulingService.GetSessionsOccurringInNextMinutes(SessionStartReminderWindowMinutes);
        Console.WriteLine($"{DateTime.Now:T} Found {sessions.Count} sessions starting in {SessionStartReminderWindowMinutes} minutes");
        foreach (var session in sessions)
        {
            var timeDiff = (session.Timestamp - DateTime.UtcNow).TotalMinutes;
            var channelToNotify = (SocketTextChannel) _client.GetChannel(session.Campaign.TextChannelId);
            
            if (timeDiff < -5) // Archive sessions older than 5 minutes that missed their reminder windows
            {
                session.State = SessionState.Archived;
                await _sessionSchedulingService.UpdateSession(session);
                await _sessionSchedulingService.CreateNextIfNecessary(session);
            }
            else if (timeDiff <= 0 && session.State == SessionState.Confirmed)
            {
                Console.WriteLine($"{DateTime.Now:T} Notifying text channel [id: {session.Campaign.TextChannelId}] of the session starting now at {session.Timestamp:g}");
                await channelToNotify.SendMessageAsync($"<@&{session.Campaign.GameMasterRoleId}>, <@&{session.Campaign.PlayerRoleId}> Attention! Today's session is about to begin!");
                session.State = SessionState.Archived;
                await _sessionSchedulingService.UpdateSession(session);
                await _sessionSchedulingService.CreateNextIfNecessary(session);
            }
            else if (session.State == SessionState.Scheduled)
            {
                Console.WriteLine($"{DateTime.Now:T} Notifying text channel [id: {session.Campaign.TextChannelId}] of the session starting in {SessionStartReminderWindowMinutes} minutes at {session.Timestamp:g}");
                await channelToNotify.SendMessageAsync($"<@&{session.Campaign.GameMasterRoleId}>, <@&{session.Campaign.PlayerRoleId}> Attention! Today's session will begin in ~30 minutes!");
                session.State = SessionState.Confirmed;
                await _sessionSchedulingService.UpdateSession(session);
            }
        }
        await RefreshTimerData();
    }
}