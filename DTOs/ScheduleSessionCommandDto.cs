using GameMasterBot.Models.Enums;

namespace GameMasterBot.DTOs;

public record ScheduleSessionCommandDto(string Date, string Time, ScheduleFrequency Frequency);