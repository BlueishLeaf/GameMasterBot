namespace GameMasterBot.DTO;

public record CampaignEntitiesDto(
    ulong TextChannelId,
    ulong VoiceChannelId,
    ulong PlayerRoleId,
    ulong GameMasterRoleId);