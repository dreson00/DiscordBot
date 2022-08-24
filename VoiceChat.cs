using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading.Channels;
using DSharpPlus.VoiceNext.EventArgs;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.IO;
using System.Net;
using DSharpPlus.CommandsNext.Converters;

namespace VocieBot
{
    public class VoiceChat
    {
        private bool _connected;
        private System.Timers.Timer aTimer;
        private VoiceNextConnection _connection;

        private DiscordClient _discord;
        private readonly PieceManager _pieceManager;
        private DiscordGuild _kanela;
        private DiscordChannel _channel;

        private List<VoicePiece> _pieceList;
        private DateTime _startRecord;
        private byte[] Zeros => Enumerable.Repeat((byte) 1, 1920).ToArray();
        private const int RemoveAndUpdateTime = 1800;


        public VoiceChat(DiscordClient discord, PieceManager pieceManager)
        {
            _discord = discord;
            _pieceManager = pieceManager;
            _connected = false;
            _pieceList = new List<VoicePiece>();

            _discord.MessageCreated += _discord_MessageCreated;
        }



        private async Task _discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            _discord.MessageCreated -= _discord_MessageCreated;
            _connection.VoiceReceived -= VoiceReceiveHandler;

            if (e.Message.Content.ToLower().StartsWith("download"))
            {
                Task.Run(() => ProceedCommand(e));

                //var msg = await new DiscordMessageBuilder()
                //    .WithFiles(new Dictionary<string, Stream>() { { "nejakynormalnijmeno.wav", stream } })
                //    .SendAsync(e.Channel);
            }
            _discord.MessageCreated += _discord_MessageCreated;
            _connection.VoiceReceived += VoiceReceiveHandler;
            //_startRecord = DateTime.Now;
        }

        private async Task ProceedCommand(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var mentions = e.Message.MentionedUsers.ToList();
            var userFilteredpieceLists = _pieceList.Where(piece => piece.User is not null)
                .GroupBy(x => x.User);
            var streams = new List<UserStream>();

            foreach (var userFilteredpieceList in userFilteredpieceLists)
            {
                var startSilenceUserFilteredpieceList = userFilteredpieceList.ToList();
                startSilenceUserFilteredpieceList.Insert(0, new VoicePiece(_startRecord, Zeros, userFilteredpieceList.Key, TimeSpan.FromMilliseconds(20)));
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
                    if (nextDuration > TimeSpan.FromSeconds(0.03))
                    {
                        var silencePiece = new VoicePiece(nextTime + TimeSpan.FromMilliseconds(1), Zeros, currentPiece.User, TimeSpan.FromMilliseconds(20));
                        pieceList.Insert(i+1, silencePiece);
                        
                        i--;
                    }
                }
            }

            return pieceList;
        }


        public async Task StartVoiceChatCheck()
        {
            _kanela = await _discord.GetGuildAsync(288667412549730304);
            _channel = await _discord.GetChannelAsync(288683278435614720);

            _discord.UseVoiceNext(new VoiceNextConfiguration()
            {
                EnableIncoming = true
            });

            SetTimer();
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            RemoveRedundantPiecesAndUpdateTime();

            if (_channel.Users.Count > 0 && !_connected)
            {
                _connection = await _channel.ConnectAsync();
                _connected = true;

                if (!Directory.Exists("Output"))
                {
                    Directory.CreateDirectory("Output");
                }

                _connection.VoiceReceived += VoiceReceiveHandler;
                _startRecord = DateTime.Now;
                Console.WriteLine($"[{_startRecord:yyyy-MM-dd-HH:mm:ss:ffff}]: Recording started");
            }
            else if (_channel.Users.Count == 1 && _connected)
            {
                _connection.VoiceReceived -= VoiceReceiveHandler;
                _connection.Disconnect();
                _connected = false;
            }
        }

        private void RemoveRedundantPiecesAndUpdateTime()
        {
            if (_connected is false || _pieceList.FirstOrDefault() is null) //DEBIL??????  "tam dej return naco to budeš obalovat" A MĚNIT PODMÍNKU UŽ NEBUDEME???
            {
                return;
            }
            var first = DateTime.Now.Subtract(_pieceList.FirstOrDefault().Time);
            if (first > TimeSpan.FromSeconds(RemoveAndUpdateTime))
            {
                _pieceList = _pieceList.Where(x => DateTime.Now.Subtract(x.Time) < TimeSpan.FromSeconds(RemoveAndUpdateTime)).ToList();
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
