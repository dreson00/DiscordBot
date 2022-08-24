using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using VocieBot;


MainAsync().GetAwaiter().GetResult();

static async Task MainAsync()
    {
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = "MTAxMTk0MDg1NzQxNjUxNTU5NA.GRFZKB.vd24nqF7sTT39ApLYm6f2kN2_ZifYVy9mZRaIM",
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


