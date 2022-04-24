using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Services.Interfaces;

public interface ISessionService
{
    Task<Session> Create(long campaignId, ScheduleFrequency scheduleFrequency, DateTime timestamp);
    Task CancelNext(long campaignId);
    Task CancelRecurringById(long sessionId);
    Task<Session> GetRecurringByCampaignId(long campaignId);
    Task<List<Session>> GetUpcomingByCampaignId(long campaignId);
}