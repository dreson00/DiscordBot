namespace VoiceBot
{
    public class VoicePiece
    {
        public DateTime Time { get; }
        public List<byte> Data { get; }
        public DSharpPlus.Entities.DiscordUser? User { get; }
        public TimeSpan Duration { get; }

        public VoicePiece(DateTime time, byte[] data, DSharpPlus.Entities.DiscordUser? user, TimeSpan duration)
        {
            Time = time;
            Data = data.ToList();
            User = user;
            Duration = duration;
        }
    }
}
