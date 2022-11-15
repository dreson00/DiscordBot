using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus;
using System.Timers;
using DSharpPlus.VoiceNext.EventArgs;
using DSharpPlus.CommandsNext;
using System.Diagnostics;
using System.IO;
using DSharpPlus.EventArgs;
using System.Threading;
using Timer = System.Timers.Timer;

namespace DiscordBot
{
    public class VoiceChat : IVoiceChat
    {
        private bool _connected;
        private System.Timers.Timer VoiceChatUserPresenceTimer;
        private VoiceNextConnection _connection;

        private DiscordClient _discord;
        private readonly IPieceManager _pieceManager;
        private DiscordGuild _kanela;
        private DiscordChannel _mainChannel;
        private DiscordChannel _additionalChannel;

        private List<VoicePiece> _pieceList;
        private DateTime _startRecord;
        private DateTime _downloadTime;
        private int _packetDuration;
        private byte[] Zeros => Enumerable.Repeat((byte)0, 1920).ToArray();
        private bool IsConnected => _discord.GetVoiceNext().GetConnection(_kanela) is not null;
        private const int RemoveAndUpdateTime = 15;

        public Timer VoiceChatBotPresenceTimer { get; set; }

        /// <summary>Initializes a new instance of the <see cref="VoiceChat" /> class.</summary>
        /// <param name="discord">The discord.</param>
        /// <param name="pieceManager">The piece manager.</param>
        public VoiceChat(DiscordClient discord, IPieceManager pieceManager)
        {
            _discord = discord;
            _pieceManager = pieceManager;
            _connected = false;
            _pieceList = new List<VoicePiece>();
            _packetDuration = 20;
            //var x = _discord.GetVoiceNext();
        }



        /// <summary>
        /// Executes the download command.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        public async Task ExecuteDownloadCommand(CommandContext ctx)
        {
            _connection.VoiceReceived -= VoiceReceiveHandler;

                await ProceedDownloadCommand(ctx);

                //var msg = await new DiscordMessageBuilder()
                //    .WithFiles(new Dictionary<string, Stream>() { { "nejakynormalnijmeno.wav", stream } })
                //    .SendAsync(e.Channel);

                _connection.VoiceReceived += VoiceReceiveHandler;
                _startRecord = DateTime.Now;
                _downloadTime = DateTime.Now;
        }

        private async Task ProceedDownloadCommand(CommandContext ctx)
        {
            var mentions = ctx.Message.MentionedUsers.ToList();
            var userFilteredpieceLists = _pieceList.Where(piece => piece.User is not null)
                .GroupBy(x => x.User);
            var streams = new List<UserStream>();

            foreach (var userFilteredpieceList in userFilteredpieceLists)
            {
                var startSilenceUserFilteredpieceList = userFilteredpieceList.ToList();
                startSilenceUserFilteredpieceList.Insert(0, new VoicePiece(_startRecord, Zeros, userFilteredpieceList.Key, TimeSpan.FromMilliseconds(_packetDuration)));
                startSilenceUserFilteredpieceList.Add(new VoicePiece(_downloadTime, Zeros, userFilteredpieceList.Key, TimeSpan.FromMilliseconds(_packetDuration)));
                streams.Add(await GetStream(
                    FillSilence(startSilenceUserFilteredpieceList), userFilteredpieceList.Key.Username));
            }


            UserStream everyoneStream;
            if (mentions.Count == 0)
            {
                everyoneStream = _pieceManager.MergePCM(streams);
            }
            else
            {
                everyoneStream = _pieceManager.MergePCM(streams.Where(stream => mentions.Exists(mention => stream.Userame == mention.Username)).ToList());
            }
        }
        private async Task<UserStream> GetStream(List<VoicePiece> pieceList, string username)
        {
            var pieceManagerOutput = await _pieceManager.WriteFile(pieceList);
            return new UserStream()
            {
                Userame = username,
                Stream = pieceManagerOutput.Item1,
                FilePath = pieceManagerOutput.Item2
            };
        }

        private List<VoicePiece> FillSilence(List<VoicePiece> pieceList)
        {
            var last = pieceList.Last();
            for (var i = 0; i < pieceList.Count; i++)
            {
                var currentPiece = pieceList[i];

                if (currentPiece.Time == last.Time)
                {
                    continue;
                }
                var nextPiece = pieceList[i + 1];
                var nextTime = currentPiece.Time + currentPiece.Duration;
                if (nextTime < nextPiece.Time)
                {
                    var nextDuration = (nextPiece.Time - currentPiece.Time - currentPiece.Duration);
                    if (nextDuration > TimeSpan.FromMilliseconds(_packetDuration + 1))
                    {
                        var silencePiece = new VoicePiece(nextTime + TimeSpan.FromMilliseconds(1), Zeros, currentPiece.User, TimeSpan.FromMilliseconds(_packetDuration));
                        pieceList.Insert(i+1, silencePiece);
                        
                        i--;
                    }
                }
            }

            return pieceList;
        }


        /// <summary>
        /// Starts the voice chat check.
        /// </summary>
        public async Task StartVoiceChatCheck()
        {
            _kanela = await _discord.GetGuildAsync(288667412549730304);
            _mainChannel = await _discord.GetChannelAsync(288683278435614720);
            _additionalChannel = await _discord.GetChannelAsync(585813017942425600);

            _discord.UseVoiceNext(new VoiceNextConfiguration()
            {
                EnableIncoming = true
            });

            SetTimer();
        }


        /// <summary>
        /// Executes the wake command.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="repeat">The repeat.</param>
        public async Task ExecuteWakeCommand(CommandContext ctx, int repeat)
        {
            var membersToDeafen = _mainChannel.Users.ToList();
            membersToDeafen.RemoveAll(x =>
                ctx.Message.MentionedUsers.ToList().Exists(y => y.Username == x.Username && !y.IsBot));
            await DeafenUsersSwitch(membersToDeafen, deaf: true);

            var pcm = File.OpenRead("out");
            pcm.Seek(0, SeekOrigin.Begin);
            pcm.Position = 0;
            var transmit = _connection.GetTransmitSink();
            await Task.WhenAll(Enumerable.Range(0, repeat)
                .Select(async _ =>
                    await TransmitAudio(pcm, transmit)
                        .ContinueWith(async _ =>
                        await Task.Delay(TimeSpan.FromMilliseconds(20)))));
            transmit = null;
            transmit?.Dispose();
            pcm.Close();
            await pcm.DisposeAsync();

            await DeafenUsersSwitch(membersToDeafen, deaf: false);
        }


        private async Task TransmitAudio(FileStream pcm, VoiceTransmitSink transmit)
        {
            await pcm.CopyToAsync(transmit);
            pcm.Position = 0;
        }

        //private Stream ConvertToPCM(string path)
        //{
        //    var ffmpeg = Process.Start(new ProcessStartInfo
        //    {
        //        FileName = "ffmpeg",
        //        Arguments = $@"-i ""{path}"" -ac 2 -f s16le -ar 48000 pipe:1",
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false
        //    });

        //    return ffmpeg.StandardOutput.BaseStream;
        //}

        private async Task DeafenUsersSwitch(List<DiscordMember> members, bool deaf)
        {
            foreach (var discordMember in members)
            {
                await discordMember.SetDeafAsync(deaf);
            }
        }


        /// <summary>
        /// Executes the irritate command.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="repeat">The repeat.</param>
        public async Task ExecuteIrritateCommand(CommandContext ctx, int repeat)
        {
            var member =
                _mainChannel.Users.Single(user => user.Username == ctx.Message.MentionedUsers.First().Username);
            if (member is not null)
            {
                for (int i = 0; i < repeat * 2; i++)
                {
                    await TransportMember(member);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        private async Task TransportMember(DiscordMember member)
        {
            if (_additionalChannel.Users.Contains(member))
            {
                await member.PlaceInAsync(_mainChannel);
            }
            else if (_mainChannel.Users.Contains(member))
            {
                await member.PlaceInAsync(_additionalChannel);
            }
        }


        private void SetTimer()
        {
            VoiceChatUserPresenceTimer = new System.Timers.Timer(1000);
            VoiceChatUserPresenceTimer.Elapsed += OnVoiceChatUserPresenceTimer;
            VoiceChatUserPresenceTimer.AutoReset = true;
            VoiceChatUserPresenceTimer.Enabled = true;

            VoiceChatBotPresenceTimer = new System.Timers.Timer(100);
            VoiceChatBotPresenceTimer.Elapsed += VoiceChatBotPresenceTimerOnElapsed;
            VoiceChatBotPresenceTimer.AutoReset = true;
            VoiceChatBotPresenceTimer.Enabled = true;
        }

        private void VoiceChatBotPresenceTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (IsConnected is false)
            {
                _connected = false;

                if (_connection is not null)
                {
                    _connection.VoiceReceived -= VoiceReceiveHandler;
                }
            }
        }

        

        private async void OnVoiceChatUserPresenceTimer(Object source, ElapsedEventArgs e)
        {
            RemoveRedundantPiecesAndUpdateTime();

            if (_mainChannel.Users.Count > 0 && _connected is false)
            {
                _connection = await _mainChannel.ConnectAsync();
                _connected = true;

                if (!Directory.Exists("Output"))
                {
                    Directory.CreateDirectory("Output");
                }

                _connection.VoiceReceived += VoiceReceiveHandler;
                _startRecord = DateTime.Now;
                Console.WriteLine($"[{_startRecord:yyyy-MM-dd-HH:mm:ss:ffff}]: Recording started");
            }
            else if (_mainChannel.Users.Count == 1 && _connected)
            {
                _connection.VoiceReceived -= VoiceReceiveHandler;
                _connection.Disconnect();
                _connected = false;
            }
        }

        private void RemoveRedundantPiecesAndUpdateTime()
        {
            if (_connected is false || _pieceList.FirstOrDefault() is null)
            {
                return;
            }
            var first = DateTime.Now.Subtract(_pieceList.FirstOrDefault().Time);
            if (first > TimeSpan.FromSeconds(RemoveAndUpdateTime))
            {
                _pieceList = _pieceList.Where(x => DateTime.Now.Subtract(x.Time) < TimeSpan.FromMinutes(RemoveAndUpdateTime)).ToList();
                _startRecord = DateTime.Now;
            }
        }

        private Task VoiceReceiveHandler(VoiceNextConnection connection, VoiceReceiveEventArgs args)
        {
            _pieceList.Add(new VoicePiece(DateTime.Now, args.PcmData.ToArray(), args.User, new TimeSpan(0, 0, 0, 0, args.AudioDuration)));
            return Task.CompletedTask;
        }
    }

    
}
