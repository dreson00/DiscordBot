using DSharpPlus.CommandsNext;

namespace VoiceBot;

public interface IVoiceChat
{
    Task ExecuteDownloadCommand(CommandContext ctx);
    Task StartVoiceChatCheck();
}