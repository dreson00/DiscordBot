using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace VocieBot.Commands
{
    public class VoiceChatAudioCommandModule : BaseCommandModule
    {
        [Command("download")]
        public async Task DownloadAudioCommand(CommandContext ctx, params string[] names)
        {
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

        }
    }
}
