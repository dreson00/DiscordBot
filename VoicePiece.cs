namespace VoiceBot
{
    public class VoicePiece
    {
        public DateTime Time { get; }
        public List<byte> Data { get; }
        public DSharpPlus.Entities.DiscordUser? User { get; }
        public TimeSpan Duration { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="VoicePiece"/> class.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="data">The data.</param>
        /// <param name="user">The user.</param>
        /// <param name="duration">The duration.</param>
        public VoicePiece(DateTime time, byte[] data, DSharpPlus.Entities.DiscordUser? user, TimeSpan duration)
        {
            Time = time;
            Data = data.ToList();
            User = user;
            Duration = duration;
        }
    }
}
