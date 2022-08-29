using DSharpPlus;
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

        [Command("wake")]
        public async Task WakeCommand(CommandContext ctx, int arg, params string[] args)
        {
            if (VoiceChat == null)
            {
                return;
            }
            var mentions = ctx.Message.MentionedUsers;
            if (mentions.Any())
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "Waking: " + string.Join(", ", mentions.Select(x => x.Username)));
                await VoiceChat.ExecuteWakeCommand(ctx, arg);
            }
            else
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "No users to wake.");
            }
            await ctx.Message.DeleteAsync();
        }

        [Command("wake")]
        public async Task WakeCommand(CommandContext ctx, params string[] _) => await WakeCommand(ctx, 1, _);

        [Command("irritate")]
        public async Task IrritateCommand(CommandContext ctx, int repeat, params string[] args)
        {
            if (VoiceChat == null)
            {
                return;
            }
            var mentions = ctx.Message.MentionedUsers;
            if (mentions.Count == 1)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "Irritating: " + mentions.First().Username);
                await VoiceChat.ExecuteIrritateCommand(ctx, repeat);
            }
            else if (mentions.Count > 1)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "I can only irritate one person at a time.");
            }
            else if (mentions.Count == 0)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel, "No users to irritate.");
            }
            await ctx.Message.DeleteAsync();
        }
        [Command("irritate")]
        public async Task IrritateCommand(CommandContext ctx, params string[] _) => await IrritateCommand(ctx, 2, _);
    }
}
