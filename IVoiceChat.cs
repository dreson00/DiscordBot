using DSharpPlus.CommandsNext;

namespace DiscordBot;

public interface IVoiceChat
{
    Task ExecuteDownloadCommand(CommandContext ctx);
    Task StartVoiceChatCheck();
    Task ExecuteWakeCommand(CommandContext ctx, int repeat);
    Task ExecuteIrritateCommand(CommandContext ctx, int repeat);
}