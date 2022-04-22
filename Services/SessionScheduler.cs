using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Services;

public class SessionScheduler
{
    private readonly DiscordSocketClient _client;
    private readonly ISessionSchedulingService _sessionSchedulingService;
    private readonly Timer _timer;
    private const int InitialDelay = 15;
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
        var sessions = await _sessionSchedulingService.GetSessionsOccurringInNextMinutes(SessionStartReminderWindowMinutes);
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

    private async void CheckSessions(object state)
    {
        var sessions = await _sessionSchedulingService.GetSessionsOccurringInNextMinutes(SessionStartReminderWindowMinutes);
        foreach (var session in sessions)
        {
            var timeDiff = (session.Timestamp - DateTime.UtcNow).TotalMinutes;
            var channelToNotify = (SocketTextChannel) _client.GetChannel(session.Campaign.TextChannelId);
            if (timeDiff <= 0 && session.State == SessionState.Confirmed)
            {
                await channelToNotify.SendMessageAsync($"<@&{session.Campaign.GameMasterRoleId}>, <@&{session.Campaign.PlayerRoleId}> Attention! Today's session is about to begin!");
                session.State = SessionState.Archived;
                await _sessionSchedulingService.CreateNextIfNecessary(session);
            }
            else if (session.State == SessionState.Scheduled)
            {
                await channelToNotify.SendMessageAsync($"<@&{session.Campaign.GameMasterRoleId}>, <@&{session.Campaign.PlayerRoleId}> Attention! Today's session will begin in ~30 minutes!");
                session.State = SessionState.Confirmed;
            }

            await _sessionSchedulingService.UpdateSession(session);
        }

        SetTimerDelay(sessions);
    }
}