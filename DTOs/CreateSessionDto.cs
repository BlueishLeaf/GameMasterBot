using System;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.DTOs;

public record CreateSessionDto(ulong CampaignId, ScheduleFrequency Frequency, DateTime UtcTime);