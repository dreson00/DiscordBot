using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VocieBot;
using VocieBot.Commands;


MainAsync().GetAwaiter().GetResult();



static async Task MainAsync()
    {
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = "MTAxMTk0MDg1NzQxNjUxNTU5NA.GRFZKB.vd24nqF7sTT39ApLYm6f2kN2_ZifYVy9mZRaIM",
            TokenType = TokenType.Bot
        });
        var services = new ServiceCollection().BuildServiceProvider();
        var commandConfig = new CommandsNextConfiguration()
        {
            Services = services,
            StringPrefixes = new[] {";"},
            CaseSensitive = false
        };
        var commands = discord.UseCommandsNext(commandConfig);
        commands.RegisterCommands<VoiceChatAudioCommandModule>();

        VoiceChat voiceChat = new VoiceChat(discord, new PieceManager(), commands);
        await voiceChat.StartVoiceChatCheck();

        await discord.ConnectAsync();
        await Task.Delay(-1);
    }


