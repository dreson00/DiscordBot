using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using VocieBot;


MainAsync().GetAwaiter().GetResult();

static async Task MainAsync()
    {
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = "MTAxMDkzMjk2MjI0NzgzNTY4OA.GP12E3.YLGEWsbN4lbClPrcmeqrCYsJHm2knktp9PhGQk",
            TokenType = TokenType.Bot
        });

        VoiceChat voiceChat = new VoiceChat(discord, new PieceManager());
        await voiceChat.StartVoiceChatCheck();

        discord.MessageCreated += async (s, e) =>
        {
            if (e.Message.Content.ToLower().StartsWith("ping"))
                await e.Message.RespondAsync("pong!");
        };

    await discord.ConnectAsync();
        await Task.Delay(-1);
    }


