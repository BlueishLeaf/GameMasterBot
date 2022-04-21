using Discord.WebSocket;

namespace GameMasterBot.DTO;

public record CreateCampaignDto(
    string Name,
    string System,
    SocketUser User,
    SocketGuild Guild,
    ulong TextChannelId,
    ulong VoiceChannelId,
    ulong PlayerRoleId,
    ulong GameMasterRoleId);