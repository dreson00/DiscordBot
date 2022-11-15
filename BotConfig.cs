namespace DiscordBot;

internal record BotConfig
{
    public BotConfig(string token, string prefix)
    {
        Token = token;
        Prefix = prefix;
    }

    public string Token { get; init; }
    public string Prefix { get; init; }
}