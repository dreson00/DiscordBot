using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using VoiceBot;
using VoiceBot.Commands;


MainAsync().GetAwaiter().GetResult();



static async Task MainAsync()
    {
        var discordConfiguration = new DiscordConfiguration()
        {
            Token = "",
            TokenType = TokenType.Bot
        };

        var discord = new DiscordClient(discordConfiguration);
        var services = new ServiceCollection()
            .AddTransient<IPieceManager, PieceManager>()
            .AddSingleton<IVoiceChat, VoiceChat>(x =>
            {
                return new VoiceChat(discord,
                        x.GetRequiredService<IPieceManager>());
            })
            .BuildServiceProvider();

        var commandConfig = new CommandsNextConfiguration()
        {
            Services = services,
            StringPrefixes = new[] {";"},
            CaseSensitive = false
        };


        var commands = discord.UseCommandsNext(commandConfig);
        commands.RegisterCommands<VoiceChatAudioCommandModule>();

        await discord.ConnectAsync();

        var voiceChatService = (IVoiceChat)services.GetService(typeof(IVoiceChat));
        await voiceChatService.StartVoiceChatCheck();

        await Task.Delay(-1);
    }


