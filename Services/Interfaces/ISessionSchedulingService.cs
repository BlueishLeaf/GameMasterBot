﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Services.Interfaces;

public interface ISessionSchedulingService
{
    Task<List<Session>> GetAllUpcomingSessions();
    Task<List<Session>> GetSessionsOccurringInNextMinutes(int minutes);
    Task<Session> UpdateSession(Session session);
    Task CreateNextIfNecessary(Session session);
}