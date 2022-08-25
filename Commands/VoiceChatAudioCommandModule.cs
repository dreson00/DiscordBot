using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace VoiceBot.Commands
{
    public class VoiceChatAudioCommandModule : BaseCommandModule
    {
        public IVoiceChat VoiceChat {private get; set; }

        [Command("download")]
        public async Task DownloadAudioCommand(CommandContext ctx, params string[] names)
        {
            if (VoiceChat == null)
            {
                return;
            }
            var mentions = ctx.Message.MentionedUsers;
            if (mentions.Any())
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "Downloading: " + string.Join(", ", mentions.Select(x => x.Username)));
            }
            else
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "Downloading");
            }
            await ctx.Message.DeleteAsync();

            await VoiceChat.ExecuteDownloadCommand(ctx);
        }
    }
}
