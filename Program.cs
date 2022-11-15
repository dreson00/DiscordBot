using System.Data;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot;
using DiscordBot.Commands;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;


MainAsync().GetAwaiter().GetResult();



static async Task MainAsync()
{
    var botConfig = TryGetBotConfig();
    var discordConfiguration = new DiscordConfiguration()
    {
        Token = botConfig.Token,
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
        StringPrefixes = new[] {botConfig.Prefix},
        CaseSensitive = false
    };


    var commands = discord.UseCommandsNext(commandConfig);
    commands.RegisterCommands<VoiceChatAudioCommandModule>();

    await discord.ConnectAsync();

    var voiceChatService = (IVoiceChat)services.GetService(typeof(IVoiceChat));
    await voiceChatService.StartVoiceChatCheck();

    await Task.Delay(-1);

    BotConfig TryGetBotConfig()
    {
        BotConfig botConfig;
        try
        {
            botConfig = JsonSerializer.Deserialize<BotConfig>(File.OpenRead("config.json")) ?? throw new NoNullAllowedException("Bot config is null.");
        }
        catch (Exception)
        {
            throw new JsonSerializationException("Unsuccessful deserialization of bot config.");
        }

        return botConfig;
    }
}