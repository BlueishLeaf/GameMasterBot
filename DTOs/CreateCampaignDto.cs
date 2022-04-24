namespace GameMasterBot.DTOs;

public record CreateCampaignDto(
    string Name,
    string System,
    ulong UserDiscordId,
    ulong GuildDiscordId,
    ulong TextChannelId,
    ulong VoiceChannelId,
    ulong PlayerRoleId,
    ulong GameMasterRoleId);