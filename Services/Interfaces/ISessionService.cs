using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameMasterBot.Models.Entities;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Services.Interfaces;

public interface ISessionService
{
    Task<Session> Create(long campaignId, ScheduleFrequency scheduleFrequency, DateTime timestamp);
    Task CancelNextByCampaignId(long campaignId);
    Task CancelAllByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime);
    Task CancelAllRecurringByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime);
    Task CancelAllByCampaignId(long campaignId);
    Task<List<Session>> GetAllRecurringByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime);
    Task<List<Session>> GetAllUpcomingByCampaignId(long campaignId);
    Task<List<Session>> GetAllByCampaignIdAndTimestamp(long campaignId, DateTime utcDateTime);
}